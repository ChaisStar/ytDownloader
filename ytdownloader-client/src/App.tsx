import { useEffect, useState } from "react";
import { DownloadItem } from "./DownloadItem";
import { format, differenceInSeconds } from "date-fns";
import { toZonedTime } from "date-fns-tz";
import { Line } from "rc-progress";
import { DownloadStatus } from "./DownloadStatus";
import * as signalR from "@microsoft/signalr";

const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;

function formatDate(date: undefined | string | Date, f: string) {
    if (!date) return "-";
    const localDate = toZonedTime(date, timeZone);
    return format(localDate, f);
}

function calculateDuration(started?: string | Date, finished?: string | Date) {
    if (!started) return "-";
    const startDate = new Date(started);
    const endDate = finished ? new Date(finished) : new Date();
    const duration = differenceInSeconds(endDate, startDate);
    const minutes = Math.floor(duration / 60);
    const seconds = duration % 60;
    return minutes > 0 ? `${minutes}m ${seconds}s` : `${seconds}s`;
}

function formatFileSize(bytes?: number | string): string {
    if (!bytes || Number.isNaN(Number(bytes))) return "-";
    const size = Number(bytes);
    const units = ["B", "KB", "MB", "GB", "TB"];
    let unitIndex = 0;
    let formattedSize = size;

    while (formattedSize >= 1024 && unitIndex < units.length - 1) {
        formattedSize /= 1024;
        unitIndex++;
    }

    return `${formattedSize.toFixed(2)} ${units[unitIndex]}`;
}

const URL = import.meta.env.VITE_API_URL || "";

const processDownloadData = (data: DownloadItem[]): DownloadItem[] => {
    const processedData = data.map(item => ({
        ...item,
        progress: Number(item.progress),
    }));
    
    // Custom sort order: Downloading, Failed, Pending, then Finished
    const statusOrder: { [key in DownloadStatus]: number } = {
        [DownloadStatus.Downloading]: 0,  // In progress first
        [DownloadStatus.Failed]: 1,        // Failed second
        [DownloadStatus.Pending]: 2,       // Processing (pending) third
        [DownloadStatus.Finished]: 3,      // Finished last
        [DownloadStatus.Cancelled]: 4,     // Cancelled at end
    };
    
    processedData.sort((a, b) => {
        const statusDiff = statusOrder[a.status] - statusOrder[b.status];
        if (statusDiff !== 0) return statusDiff;
        
        // Within same status, prioritize items without "later" flag
        if (a.later !== b.later) {
            return a.later ? 1 : -1;  // false (later: false) comes first
        }
        
        // Within same status and later flag, show newest first
        return new Date(b.created).getTime() - new Date(a.created).getTime();
    });

    return processedData;
};

function App() {
    const [downloads, setDownloads] = useState<DownloadItem[]>([]);
    const [archivedDownloads, setArchivedDownloads] = useState<DownloadItem[]>([]);
    const [url, setUrl] = useState<string>("");
    const [later, setLater] = useState<boolean>(false);
    const [expandedId, setExpandedId] = useState<number | null>(null);
    const [ytDlpVersion, setYtDlpVersion] = useState<string>("");
    const [cookiesInfo, setCookiesInfo] = useState<{ lastModified?: string; size: number; exists: boolean }>({ size: 0, exists: false });
    const [isUpdating, setIsUpdating] = useState(false);
    const [activeTab, setActiveTab] = useState<"current" | "archive">("current");

    // SignalR connection effect
    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`${URL}/hub/downloads`)
            .withAutomaticReconnect()
            .build();

        connection.start()
            .then(() => console.log("Connected to SignalR"))
            .catch(err => console.error("SignalR connection failed:", err));

        // Listen for downloads updates
        connection.on("ReceiveDownloadsUpdate", (data: DownloadItem[]) => {
            setDownloads(processDownloadData(data));
        });

        // Listen for version updates
        connection.on("ReceiveVersionUpdate", (version: string) => {
            setYtDlpVersion(version);
        });

        // Listen for cookies updates
        connection.on("ReceiveCookiesUpdate", (info: { lastModified?: string; size: number; exists: boolean }) => {
            setCookiesInfo(info);
        });

        // Fetch initial data on connect
        const fetchInitialData = async () => {
            try {
                const currentResponse = await fetch(`${URL}/downloads`);
                const currentData: DownloadItem[] = await currentResponse.json();
                setDownloads(processDownloadData(currentData));

                const versionResponse = await fetch(`${URL}/ytdlp/version`);
                if (versionResponse.ok) {
                    const version = await versionResponse.text();
                    setYtDlpVersion(version);
                }

                const cookiesResponse = await fetch(`${URL}/cookies/info`);
                if (cookiesResponse.ok) {
                    const info = await cookiesResponse.json();
                    setCookiesInfo(info);
                }
            } catch (error) {
                console.error("Error fetching initial data:", error);
            }
        };

        fetchInitialData();

        return () => {
            connection.stop();
        };
    }, []);

    useEffect(() => {
        const fetchArchive = async () => {
            if (activeTab === "archive" && archivedDownloads.length === 0) {
                try {
                    const archiveResponse = await fetch(`${URL}/archive`);
                    const archiveData: DownloadItem[] = await archiveResponse.json();
                    setArchivedDownloads(processDownloadData(archiveData));
                } catch (error) {
                    console.error("Error fetching archived downloads:", error);
                }
            }
        };

        fetchArchive();
    }, [activeTab]);

    const handleDelete = async (id: number) => {
        try {
            const response = await fetch(`${URL}/downloads/${id}`, {
                method: "DELETE",
            });

            if (!response.ok) {
                throw new Error("Failed to delete download");
            }

            setDownloads(prevDownloads => 
                prevDownloads.filter(download => download.id !== id)
            );
        } catch (error) {
            console.error("Error deleting download:", error);
            alert("Failed to delete download");
        }
    };

    const handleAdd = async () => {
    try {
        const response = await fetch(`${URL}/downloads`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                    url,
                    later,
                }),
        });

        if (!response.ok) {
            throw new Error("Failed to add download");
        }

        // Оновлення списку після додавання
        setUrl("");
        setLater(false);
    } catch (error) {
        console.error("Error adding download:", error);
        alert("Failed to add download");
    }
    };

    const handleUpdateYtDlp = async () => {
        setIsUpdating(true);
        try {
            const response = await fetch(`${URL}/ytdlp/update`, {
                method: "POST",
            });

            if (!response.ok) {
                throw new Error("Failed to update yt-dlp");
            }

            const result = await response.json();
            alert(`yt-dlp updated successfully!\n${result.message}`);
            
            // Refresh version after update
            const versionResponse = await fetch(`${URL}/ytdlp/version`);
            if (versionResponse.ok) {
                const version = await versionResponse.text();
                setYtDlpVersion(version);
            }
        } catch (error) {
            console.error("Error updating yt-dlp:", error);
            alert("Failed to update yt-dlp");
        } finally {
            setIsUpdating(false);
        }
    };

    const handleDeleteArchive = async () => {
        if (!globalThis.confirm("Are you sure you want to delete all archived downloads? This action cannot be undone.")) {
            return;
        }

        try {
            const response = await fetch(`${URL}/archive`, {
                method: "DELETE",
            });

            if (!response.ok) {
                throw new Error("Failed to delete archived downloads");
            }

            const result = await response.json();
            alert(result.message);
            
            // Clear archived downloads from UI
            setArchivedDownloads([]);
        } catch (error) {
            console.error("Error deleting archived downloads:", error);
            alert("Failed to delete archived downloads");
        }
    };

    // Helper function to get status display text and color
    const getStatusDisplay = (status: DownloadStatus) => {
        switch (status) {
            case DownloadStatus.Pending:
                return { text: "Pending", className: "text-yellow-600" };
            case DownloadStatus.Downloading:
                return { text: "Downloading", className: "text-blue-600" };
            case DownloadStatus.Finished:
                return { text: "Finished", className: "text-green-600" };
            case DownloadStatus.Failed:
                return { 
                    text: "Failed", 
                    className: "text-red-600" 
                };
            case DownloadStatus.Cancelled:
                return { text: "Cancelled", className: "text-gray-600" };
            default:
                return { text: "Unknown", className: "text-gray-600" };
        }
    };

    return (
        <div className="min-h-screen bg-gray-50 p-3 md:p-6">
            {/* Header */}
            <div className="mb-4 md:mb-6">
                <h1 className="text-2xl md:text-3xl font-bold text-gray-900 mb-3 md:mb-4">YouTube Downloader</h1>
                <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-3 md:p-4">
                    <div className="flex flex-col gap-2">
                        <input
                            type="text"
                            value={url}
                            onChange={(e) => setUrl(e.target.value)}
                            placeholder="Enter YouTube URL"
                            className="border border-gray-300 px-3 py-2 rounded text-sm md:text-base focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200"
                        />
                        <div className="flex items-center justify-between gap-2">
                            <label className="flex items-center space-x-2 text-gray-700 font-medium text-sm md:text-base">
                                <input
                                    type="checkbox"
                                    checked={later}
                                    onChange={() => setLater((current) => !current)}
                                    className="w-4 h-4 rounded border-gray-300 cursor-pointer"
                                />
                                <span>Later</span>
                            </label>
                            <button
                                onClick={handleAdd}
                                className="bg-blue-500 hover:bg-blue-600 text-white font-semibold py-2 px-4 text-sm md:text-base rounded flex-1"
                            >
                                Add
                            </button>
                        </div>
                    </div>
                </div>
            </div>

            {/* Info Section - Hidden on mobile */}
            <div className="hidden md:flex flex-row gap-2 mb-6 text-xs">
                <div className="flex items-center space-x-2 bg-gray-100 border border-gray-300 rounded px-3 py-2">
                    <span className="text-blue-600 font-bold">⬆</span>
                    <div className="flex-1">
                        <p className="text-gray-600 font-medium">yt-dlp: {ytDlpVersion || "..."}</p>
                    </div>
                    <button
                        onClick={handleUpdateYtDlp}
                        disabled={isUpdating}
                        className="bg-green-500 hover:bg-green-600 disabled:bg-gray-400 text-white font-semibold py-0.5 px-2 rounded text-xs whitespace-nowrap"
                    >
                        {isUpdating ? "..." : "Update"}
                    </button>
                </div>

                <div className="flex items-center space-x-2 bg-gray-100 border border-gray-300 rounded px-3 py-2 flex-1">
                    <span className="text-green-600 font-bold">🍪</span>
                    <div className="flex-1 min-w-0">
                        {cookiesInfo.exists ? (
                            <p className="text-gray-700 truncate">
                                Cookies: {cookiesInfo.lastModified && <span>{new Date(cookiesInfo.lastModified).toLocaleDateString()}</span>}
                            </p>
                        ) : (
                            <p className="text-gray-700">Cookies: Not configured</p>
                        )}
                    </div>
                </div>
            </div>

            {/* Tab Navigation */}
            <div className="mb-4 md:mb-6 flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2 border-b-2 border-gray-200">
                <div className="flex space-x-2 w-full sm:w-auto">
                    <button
                        onClick={() => setActiveTab("current")}
                        className={`px-3 md:px-4 py-2 md:py-3 text-sm md:text-base font-semibold transition-colors duration-200 border-b-2 ${activeTab === "current" ? "text-blue-600 border-blue-600" : "text-gray-600 hover:text-gray-900 border-transparent"}`}
                    >
                        Current ({downloads.length})
                    </button>
                    <button
                        onClick={() => setActiveTab("archive")}
                        className={`px-3 md:px-4 py-2 md:py-3 text-sm md:text-base font-semibold transition-colors duration-200 border-b-2 ${activeTab === "archive" ? "text-blue-600 border-blue-600" : "text-gray-600 hover:text-gray-900 border-transparent"}`}
                    >
                        Archive ({archivedDownloads.length})
                    </button>
                </div>
                {activeTab === "archive" && archivedDownloads.length > 0 && (
                    <button
                        onClick={handleDeleteArchive}
                        className="bg-red-600 hover:bg-red-700 text-white font-semibold py-2 px-3 text-sm rounded w-full sm:w-auto"
                    >
                        Delete All
                    </button>
                )}
            </div>
            <div className="overflow-x-auto rounded-lg shadow-md border border-gray-200">
                <table className="min-w-full divide-y divide-gray-200 text-xs md:text-sm">
                    <thead className="bg-gradient-to-r from-gray-800 to-gray-700">
                        <tr>
                            {["", "Thumbnail", "Title", "Status", "Size", "Created", "Duration", "Later", "Action"].map((header) => (
                                <th key={header} className={`px-2 md:px-4 py-2 md:py-3 font-semibold text-white text-left uppercase tracking-wide ${["Thumbnail", "Created", "Duration", "Later"].includes(header) ? "hidden md:table-cell" : ""}`}>
                                    {header}
                                </th>
                            ))}
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200 bg-white">
                        {(activeTab === "current" ? downloads : archivedDownloads).map((item) => {
                            const statusDisplay = getStatusDisplay(item.status);
                            const isExpanded = expandedId === item.id;
                            return (
                                <>
                                <tr key={item.id} className="hover:bg-blue-50 transition-colors">
                                    <td className="px-2 md:px-4 py-2 md:py-3 text-center">
                                        <button
                                            onClick={() => setExpandedId(isExpanded ? null : item.id)}
                                            className="text-blue-600 hover:text-blue-800 font-bold cursor-pointer transition-colors"
                                            title={item.errorMessage ? "Click to view error details" : "No details"}
                                            type="button"
                                        >
                                            {isExpanded ? "▼" : "▶"}
                                        </button>
                                    </td>
                                    <td className="px-2 md:px-4 py-2 md:py-3 hidden md:table-cell">
                                        <a href={item.url} target="_blank" rel="noopener noreferrer" className="text-blue-600 hover:text-blue-800 underline transition-colors">
                                            {item.thumbnail ? <img src={item.thumbnail} alt="Thumbnail" className="w-16 h-auto rounded" /> : "N/A"}
                                        </a>
                                    </td>
                                    <td className="px-2 md:px-4 py-2 md:py-3 max-w-xs truncate text-gray-900 font-medium">{item.title ?? item.url}</td>
                                    <td className="px-2 md:px-4 py-2 md:py-3">
                                        {item.status === DownloadStatus.Downloading ? (
                                            <div className="flex flex-col space-y-1">
                                                <div className="flex items-center space-x-1 md:space-x-2">
                                                    <Line 
                                                        percent={item.progress} 
                                                        strokeWidth={2} 
                                                        strokeColor="#4CAF50"
                                                        trailColor="#D9D9D9"
                                                        style={{ width: "60px" }}
                                                    />
                                                    <span className="font-semibold text-xs">{item.progress}%</span>
                                                </div>
                                                <div className="text-gray-700 text-xs hidden md:block">Speed: {item.speed ?? "-"}</div>
                                                <div className="text-gray-700 text-xs hidden md:block">ETA: {item.eta ?? "-"}</div>
                                            </div>
                                        ) : (
                                            <span className={`font-semibold ${statusDisplay.className}`}>
                                                {statusDisplay.text}
                                            </span>
                                        )}
                                    </td>
                                    <td className="px-2 md:px-4 py-2 md:py-3 font-medium text-gray-900">{formatFileSize(item.totalSize)}</td>
                                    <td className="px-2 md:px-4 py-2 md:py-3 text-gray-700 hidden md:table-cell">{formatDate(item.created, "dd-MM HH:mm")}</td>
                                    <td className="px-2 md:px-4 py-2 md:py-3 text-gray-700 hidden md:table-cell">{calculateDuration(item.started, item.finished)}</td>
                                    <td className="px-2 md:px-4 py-2 md:py-3 text-center hidden md:table-cell">{item.later ? <span className="inline-block bg-yellow-100 text-yellow-800 px-2 py-1 rounded text-xs font-semibold">Later</span> : ""}</td>
                                    <td className="px-2 md:px-4 py-2 md:py-3">
                                        <button
                                            onClick={() => handleDelete(item.id)}
                                            className="bg-red-600 hover:bg-red-700 text-white font-semibold py-1 px-2 text-xs rounded"
                                        >
                                            Delete
                                        </button>
                                    </td>
                                </tr>
                                {isExpanded && item.errorMessage && (
                                    <tr className="bg-red-50 hover:bg-red-100">
                                        <td colSpan={9} className="px-2 md:px-4 py-3 md:py-4">
                                            <div className="bg-red-50 border-l-4 border-red-600 p-4 rounded text-red-800">
                                                <div className="font-semibold mb-2 text-red-900">Error Details:</div>
                                                <div className="whitespace-pre-wrap text-sm font-mono bg-red-100 p-3 rounded border border-red-300 overflow-auto max-h-48 text-red-800">
                                                    {item.errorMessage}
                                                </div>
                                            </div>
                                        </td>
                                    </tr>
                                )}
                                </>
                            );
                        })}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

export default App;