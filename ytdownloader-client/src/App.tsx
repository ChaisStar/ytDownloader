import { useEffect, useState } from "react";
import { DownloadItem } from "./DownloadItem";
import { format, differenceInSeconds } from "date-fns";
import { toZonedTime } from "date-fns-tz";
import { Line } from "rc-progress";
import { DownloadStatus } from "./DownloadStatus";
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
    return `${Math.floor(duration / 60)}m ${duration % 60}s`;
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

function App() {
    const [downloads, setDownloads] = useState<DownloadItem[]>([]);
    const [url, setUrl] = useState<string>("");
    const [later, setLater] = useState<boolean>(false);
    const [expandedId, setExpandedId] = useState<number | null>(null);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const response = await fetch(`${URL}/downloads`);
                const data: DownloadItem[] = await response.json();
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
                
                setDownloads(processedData);
            } catch (error) {
                console.error("Error fetching download status:", error);
            }
        };

        fetchData();
        const interval = setInterval(fetchData, 1000);
        return () => clearInterval(interval);
    }, []);

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
        <div className="p-4">
            <div className="min-w-full flex flex-row items-center justify-between space-x-2 mb-4">
                <h1 className="text-xl font-bold">Downloads Status</h1>
                <div className="flex flex-row items-center space-x-2">
                    <input
                        type="text"
                        value={url}
                        onChange={(e) => setUrl(e.target.value)}
                        placeholder="Enter URL"
                        className="border px-2 py-1 rounded"
                    />
                    <label className="flex items-center space-x-1">
                        <input
                            type="checkbox"
                            checked={later}
                            onChange={() => setLater((current) => !current)}
                        />
                        <span>Later</span>
                    </label>
                    <button
                        onClick={handleAdd}
                        className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-1 px-3 rounded text-sm"
                    >
                        Add
                    </button>
                </div>
            </div>
            <div className="overflow-x-auto">
                <table className="min-w-full border border-gray-300">
                    <thead className="bg-gray-100">
                        <tr>
                            {["", "Thumbnail", "Title", "Status", "Size",
                                "Created", "Duration", "Later", "Action"
                            ].map((header) => (
                                <th key={header} className="border border-gray-300 px-3 py-2 text-sm text-left">
                                    {header}
                                </th>
                            ))}
                        </tr>
                    </thead>
                    <tbody>
                        {downloads.map((item) => {
                            const statusDisplay = getStatusDisplay(item.status);
                            const isExpanded = expandedId === item.id;
                            return (
                                <>
                                <tr key={item.id} className="hover:bg-gray-50">
                                    <td className="border border-gray-300 px-3 py-2 text-center">
                                        <button
                                            onClick={() => setExpandedId(isExpanded ? null : item.id)}
                                            className="text-blue-600 hover:text-blue-800 font-bold cursor-pointer w-6 h-6"
                                            title={item.errorMessage ? "Click to view error details" : "No details"}
                                            type="button"
                                        >
                                            {isExpanded ? "▼" : "▶"}
                                        </button>
                                    </td>
                                    <td className="border border-gray-300 px-3 py-2">
                                        <a href={item.url} target="_blank" rel="noopener noreferrer" className="text-blue-600 underline">
                                            {item.thumbnail ? <img src={item.thumbnail} alt="Thumbnail" className="w-32 h-auto" /> : "N/A"}
                                        </a>
                                    </td>
                                    <td className="border border-gray-300 px-3 py-2">{item.title ?? item.url}</td>
                                    <td className="border border-gray-300 px-3 py-2">
                                        {item.status === DownloadStatus.Downloading ? (
                                            <div className="flex flex-col space-y-1">
                                                <div className="flex items-center">
                                                    <Line 
                                                        percent={item.progress} 
                                                        strokeWidth={2} 
                                                        strokeColor="#4CAF50"
                                                        trailColor="#D9D9D9"
                                                        style={{ width: "100px" }}
                                                    />
                                                    <span className="ml-2">{item.progress}%</span>
                                                </div>
                                                <div>Speed: {item.speed ?? "-"}</div>
                                                <div>ETA: {item.eta ?? "-"}</div>
                                            </div>
                                        ) : (
                                            <span className={statusDisplay.className}>
                                                {statusDisplay.text}
                                            </span>
                                        )}
                                    </td>
                                    <td className="border border-gray-300 px-3 py-2">{formatFileSize(item.totalSize)}</td>
                                    <td className="border border-gray-300 px-3 py-2">{formatDate(item.created, "dd-MM HH:mm")}</td>
                                    <td className="border border-gray-300 px-3 py-2">{calculateDuration(item.started, item.finished)}</td>
                                    <td className="border border-gray-300 px-3 py-2">{item.later ? "✓" : ""}</td>
                                    <td className="border border-gray-300 px-3 py-2">
                                        <button
                                            onClick={() => handleDelete(item.id)}
                                            className="bg-red-500 hover:bg-red-700 text-white font-bold py-1 px-2 rounded text-sm"
                                        >
                                            Delete
                                        </button>
                                    </td>
                                </tr>
                                {isExpanded && item.errorMessage && (
                                    <tr className="bg-red-50">
                                        <td colSpan={9} className="border border-gray-300 px-3 py-2">
                                            <div className="text-red-800">
                                                <div className="font-semibold mb-1">Error Details:</div>
                                                <div className="whitespace-pre-wrap text-sm font-mono bg-red-100 p-2 rounded border border-red-300 overflow-auto max-h-40">
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