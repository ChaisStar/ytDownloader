import { useEffect, useState } from "react";
import { DownloadItem } from "./DownloadItem";
import { format } from "date-fns";
import { toZonedTime } from "date-fns-tz";
import { DownloadStatus } from "./DownloadStatus";

const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;

function formatDate(date: undefined | string | Date, f: string) {
    if (!date) return "N/A";
    const localDate = toZonedTime(date, timeZone);
    return format(localDate, f);
}

const URL = import.meta.env.VITE_API_URL || "";

function App() {
    const [downloads, setDownloads] = useState<DownloadItem[]>([]);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const response = await fetch(`${URL}/downloads`);
                const data: DownloadItem[] = await response.json();
                setDownloads(data.map(item => ({
                    ...item,
                    progress: Number(item.progress),
                })));
            } catch (error) {
                console.error("Error fetching download status:", error);
            }
        };

        fetchData();
        const interval = setInterval(fetchData, 1000);
        return () => clearInterval(interval);
    }, []);

    return (
        <div className="p-4">
            <h1 className="text-xl font-bold mb-4">Downloads Status</h1>
            <div className="overflow-x-auto">
                <table className="min-w-full border border-gray-300">
                    <thead className="bg-gray-100">
                        <tr>
                            {[
                                "Thumbnail", "Title", "Status", "Progress", "Size",
                                "Speed", "ETA", "Created", "Started", "Finished", "Later"
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
                                <td className="border border-gray-300 px-3 py-2">{item.progress}%</td>
                                <td className="border border-gray-300 px-3 py-2">{item.totalSize ?? "N/A"}</td>
                                <td className="border border-gray-300 px-3 py-2">{item.speed ?? "N/A"}</td>
                                <td className="border border-gray-300 px-3 py-2">{item.eta ?? "N/A"}</td>
                                <td className="border border-gray-300 px-3 py-2">{formatDate(item.created, "dd-MM HH:mm")}</td>
                                <td className="border border-gray-300 px-3 py-2">{formatDate(item.started, "HH:mm")}</td>
                                <td className="border border-gray-300 px-3 py-2">{formatDate(item.finished, "HH:mm")}</td>
                                <td className="border border-gray-300 px-3 py-2">{item.later ? "✓" : ""}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

export default App;