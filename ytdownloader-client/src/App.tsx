import { useEffect, useState } from "react";
import { DownloadItem } from "./DownloadItem";
import { format, differenceInSeconds } from "date-fns";
import { toZonedTime } from "date-fns-tz";
import { DownloadStatus } from "./DownloadStatus";
import { Line } from "rc-progress";

const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;

function formatDate(date: undefined | string | Date, f: string) {
    if (!date) return "-";
    const localDate = toZonedTime(date, timeZone);
    return format(localDate, f);
}

function calculateDuration(started?: string | Date, finished?: string | Date) {
    if (!started) return "-";
    const end = finished ? new Date(finished) : new Date();
    const duration = differenceInSeconds(end, new Date(started));
    return `${Math.floor(duration / 60)}m ${duration % 60}s`;
}

const URL = import.meta.env.VITE_API_URL || "";

function App() {
    const [downloads, setDownloads] = useState<DownloadItem[]>([]);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const response = await fetch(`${URL}/downloads`);
                const data: DownloadItem[] = await response.json();
                const processedData = data.map(item => ({
                    ...item,
                    progress: Number(item.progress),
                }));
                processedData.sort((a, b) => 
                    new Date(b.created).getTime() - new Date(a.created).getTime()
                );
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

            // Update state by removing the deleted item
            setDownloads(prevDownloads =>
                prevDownloads.filter(download => download.id !== id)
            );
        } catch (error) {
            console.error("Error deleting download:", error);
            // Optionally show an error message to the user
            alert("Failed to delete download");
        }
    };


    return (
        <div className="p-4">
            <h1 className="text-xl font-bold mb-4">Downloads Status</h1>
            <div className="overflow-x-auto">
                <table className="min-w-full border border-gray-300">
                    <thead className="bg-gray-100">
                        <tr>
                            {["Thumbnail", "Title", "Status", "Progress", "Size",
                                "Speed", "ETA", "Created", "Duration", "Later", ""
                            ].map((header) => (
                                <th key={header} className="border border-gray-300 px-3 py-2 text-sm text-left">
                                    {header}
                                </th>
                            ))}
                        </tr>
                    </thead>
                    <tbody>
                        {downloads.map((item) => (
                            <tr key={item.id} className="hover:bg-gray-50">
                                <td className="border border-gray-300 px-3 py-2">
                                    <a href={item.url} target="_blank" rel="noopener noreferrer" className="text-blue-600 underline">
                                        {item.thumbnail ? <img src={item.thumbnail} alt="Thumbnail" className="w-32 h-auto" /> : "N/A"}
                                    </a>
                                </td>
                                <td className="border border-gray-300 px-3 py-2">{item.title ?? "Undefined"}</td>
                                <td className="border border-gray-300 px-3 py-2">{DownloadStatus[item.status]}</td>
                                <td className="border border-gray-300 px-3 py-2">
                                    <Line
                                        percent={item.progress}
                                        strokeWidth={2}
                                        strokeColor="#4CAF50" // Green color for progress
                                        trailColor="#D9D9D9" // Gray color for the trail
                                        style={{ width: "100px" }} // Adjust width as needed
                                    />
                                    <span className="ml-2">{item.progress}%</span> {/* Optional: keep percentage text */}
                                </td>
                                <td className="border border-gray-300 px-3 py-2">{item.totalSize ?? "-"}</td>
                                <td className="border border-gray-300 px-3 py-2">{item.speed ?? "-"}</td>
                                <td className="border border-gray-300 px-3 py-2">{item.eta ?? "-"}</td>
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
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

export default App;
