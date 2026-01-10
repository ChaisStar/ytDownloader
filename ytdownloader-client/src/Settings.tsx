import { format } from "date-fns";
import { OptionSetManager } from "./OptionSetManager";
import { TagManager } from "./TagManager";
import { Tag } from "./DownloadItem";

interface SettingsProps {
    ytDlpVersion: string;
    isUpdating: boolean;
    onUpdateYtDlp: () => Promise<void>;
    cookiesInfo: {
        lastModified?: string;
        size: number;
        exists: boolean;
    };
    tags: Tag[];
    onTagsRefresh: () => void;
}

export function Settings({ 
    ytDlpVersion, 
    isUpdating, 
    onUpdateYtDlp, 
    cookiesInfo,
    tags,
    onTagsRefresh 
}: SettingsProps) {
    return (
        <div className="space-y-6">
            <h2 className="text-xl font-bold text-gray-900 px-1">System Settings</h2>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {/* yt-dlp Management Card */}
                <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden flex flex-col">
                    <div className="p-3 border-b border-gray-100 bg-gray-50/50">
                        <h3 className="font-bold text-gray-900 flex items-center gap-2 text-sm">
                            <span className="p-1 bg-blue-100 text-blue-600 rounded text-base">⬇️</span>
                            Downloader Core
                        </h3>
                    </div>
                    <div className="p-4 flex-1 flex flex-col justify-between">
                        <div className="space-y-3 mb-4">
                            <div>
                                <span className="text-[10px] font-bold uppercase tracking-wider text-gray-400 block mb-0.5">Engine</span>
                                <p className="text-gray-900 font-medium flex items-center gap-2 text-sm">
                                    yt-dlp
                                    {" "}<span className="px-1.5 py-0 bg-blue-50 text-blue-700 text-[10px] rounded-full border border-blue-100">Active</span>
                                </p>
                            </div>
                            <div>
                                <span className="text-[10px] font-bold uppercase tracking-wider text-gray-400 block mb-0.5">Installed Version</span>
                                <p className="font-mono text-gray-700 bg-gray-50 px-1.5 py-0.5 rounded border border-gray-200 inline-block text-xs">
                                    {ytDlpVersion || "Scanning..."}
                                </p>
                            </div>
                        </div>
                        
                        <button
                            onClick={onUpdateYtDlp}
                            disabled={isUpdating}
                            className="w-full bg-gray-900 hover:bg-black disabled:bg-gray-400 text-white font-bold py-2 px-3 rounded text-xs transition-all duration-200 flex items-center justify-center gap-2 shadow-sm cursor-pointer"
                        >
                            {isUpdating ? (
                                <>
                                    <svg className="animate-spin h-3.5 w-3.5 text-white" viewBox="0 0 24 24">
                                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                                    </svg>
                                    <span>Syncing...</span>
                                </>
                            ) : (
                                <>
                                    <span>Update Downloader</span>
                                    <span className="text-sm">⚡</span>
                                </>
                            )}
                        </button>
                    </div>
                </div>

                {/* Authentication & Cookies Card */}
                <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden flex flex-col">
                    <div className="p-3 border-b border-gray-100 bg-gray-50/50">
                        <h3 className="font-bold text-gray-900 flex items-center gap-2 text-sm">
                            <span className="p-1 bg-orange-100 text-orange-600 rounded text-base">🍪</span>
                            Authentication
                        </h3>
                    </div>
                    <div className="p-4 flex-1">
                        <div className="space-y-3">
                            <div>
                                <span className="text-[10px] font-bold uppercase tracking-wider text-gray-400 block mb-0.5">Cookie Credentials</span>
                                {cookiesInfo.exists ? (
                                    <div className="flex items-center gap-2">
                                        <div className="h-2 w-2 rounded-full bg-green-500 animate-pulse"></div>
                                        <span className="font-medium text-green-700 text-sm">Active</span>
                                    </div>
                                ) : (
                                    <div className="flex items-center gap-2 text-red-600 font-medium text-sm">
                                        <div className="h-2 w-2 rounded-full bg-red-500"></div>
                                        <span>Missing</span>
                                    </div>
                                )}
                            </div>

                            <div className="p-3 bg-gray-50 rounded border border-gray-200 space-y-2">
                                <div className="flex justify-between items-center text-[11px]">
                                    <span className="text-gray-400">Integrity:</span>
                                    <span className={cookiesInfo.exists ? "text-green-600 font-bold" : "text-red-600 font-bold"}>
                                        {cookiesInfo.exists ? "Verified" : "Failed"}
                                    </span>
                                </div>
                                <div className="flex justify-between items-center text-[11px]">
                                    <span className="text-gray-400">Synced:</span>
                                    <span className="text-gray-900 font-medium">
                                        {cookiesInfo.exists && cookiesInfo.lastModified 
                                            ? format(new Date(cookiesInfo.lastModified), "MMM dd, HH:mm")
                                            : "Never"}
                                    </span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 items-start">
                <OptionSetManager />
                <TagManager tags={tags} onRefresh={onTagsRefresh} />
            </div>
            
            {/* System Status Banner */}
            <div className="bg-blue-600 rounded-lg p-3 text-white shadow-md flex items-center justify-between">
                <div className="flex items-center gap-3">
                    <div className="p-2 bg-white/20 rounded-full text-lg">🌐</div>
                    <div>
                        <h4 className="font-bold text-sm">System Ready</h4>
                        <p className="text-blue-100 text-[10px]">Background workers operational.</p>
                    </div>
                </div>
                <div className="hidden sm:block text-2xl opacity-20 select-none">✓</div>
            </div>
        </div>
    );
}
