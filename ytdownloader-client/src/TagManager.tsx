import { useState } from "react";
import { Tag, TagUsage } from "./DownloadItem";

const URL = import.meta.env.VITE_API_URL || "";

interface TagManagerProps {
    readonly tags: Tag[];
    readonly onRefresh: () => void;
}

export function TagManager({ tags, onRefresh }: TagManagerProps) {
    const [editingId, setEditingId] = useState<number | null>(null);
    const [tempTag, setTempTag] = useState<Partial<Tag> | null>(null);

    const startEdit = (tag: Tag) => {
        setEditingId(tag.id);
        setTempTag({ ...tag });
    };

    const saveEdit = async () => {
        if (!tempTag || !editingId) return;
        try {
            await fetch(`${URL}/tags/${editingId}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(tempTag),
            });
            setEditingId(null);
            onRefresh();
        } catch (e) {
            console.error(e);
        }
    };

    const deleteTag = async (id: number) => {
        if (!confirm("Are you sure?")) return;
        try {
            await fetch(`${URL}/tags/${id}`, { method: "DELETE" });
            onRefresh();
        } catch (e) {
            console.error(e);
        }
    };

    const addTag = async () => {
        const newTag: Partial<Tag> = {
            name: "New Tag",
            value: "folder_or_prefix",
            usage: TagUsage.Directory,
            color: "#6366f1",
        };
        try {
            await fetch(`${URL}/tags`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(newTag),
            });
            onRefresh();
        } catch (e) {
            console.error(e);
        }
    };

    return (
        <div className="bg-white rounded-xl shadow-lg border border-gray-200 overflow-hidden">
            <div className="p-4 border-b border-gray-100 flex justify-between items-center">
                <h3 className="font-bold text-lg">Tag Management</h3>
                <button
                    onClick={addTag}
                    className="bg-indigo-600 text-white px-3 py-1.5 rounded-lg text-sm font-bold shadow-sm hover:bg-indigo-700 transition"
                >
                    + Add Tag
                </button>
            </div>
            <div className="p-4 space-y-4">
                {tags.map((tag) => (
                    <div key={tag.id} className="border rounded-xl p-4 bg-gray-50/50">
                        {editingId === tag.id ? (
                            <div className="space-y-3">
                                <div className="grid grid-cols-2 gap-3">
                                    <div>
                                        <label htmlFor={`name-${tag.id}`} className="text-[10px] font-bold uppercase text-gray-500">Name</label>
                                        <input
                                            id={`name-${tag.id}`}
                                            className="w-full text-sm p-2 border rounded-lg"
                                            value={tempTag?.name}
                                            onChange={(e) => setTempTag({ ...tempTag, name: e.target.value })}
                                        />
                                    </div>
                                    <div>
                                        <label htmlFor={`value-${tag.id}`} className="text-[10px] font-bold uppercase text-gray-500">Value</label>
                                        <input
                                            id={`value-${tag.id}`}
                                            className="w-full text-sm p-2 border rounded-lg"
                                            value={tempTag?.value}
                                            onChange={(e) => setTempTag({ ...tempTag, value: e.target.value })}
                                        />
                                    </div>
                                </div>
                                <div className="grid grid-cols-2 gap-3">
                                    <div>
                                        <label htmlFor={`usage-${tag.id}`} className="text-[10px] font-bold uppercase text-gray-500">Usage</label>
                                        <select
                                            id={`usage-${tag.id}`}
                                            className="w-full text-sm p-2 border rounded-lg"
                                            value={tempTag?.usage}
                                            onChange={(e) => setTempTag({ ...tempTag, usage: Number(e.target.value) })}
                                        >
                                            <option value={TagUsage.Directory}>New Directory</option>
                                            <option value={TagUsage.Prefix}>Filename Prefix</option>
                                            <option value={TagUsage.Suffix}>Filename Suffix</option>
                                        </select>
                                    </div>
                                    <div>
                                        <label htmlFor={`color-${tag.id}`} className="text-[10px] font-bold uppercase text-gray-500">Color</label>
                                        <input
                                            id={`color-${tag.id}`}
                                            type="color"
                                            className="w-full h-9 p-1 border rounded-lg"
                                            value={tempTag?.color}
                                            onChange={(e) => setTempTag({ ...tempTag, color: e.target.value })}
                                        />
                                    </div>
                                </div>
                                <div className="flex justify-end gap-2 pt-2">
                                    <button onClick={() => setEditingId(null)} className="text-sm text-gray-500 font-bold px-3">Discard</button>
                                    <button onClick={saveEdit} className="bg-indigo-600 text-white text-sm font-bold px-4 py-1.5 rounded-lg">Save</button>
                                </div>
                            </div>
                        ) : (
                            <div className="flex justify-between items-center">
                                <div className="flex items-center gap-3">
                                    <div 
                                        className="w-4 h-4 rounded-full shadow-inner" 
                                        style={{ backgroundColor: tag.color || '#ccc' }} 
                                    />
                                    <div>
                                        <div className="font-bold flex items-center gap-2">
                                            {tag.name}
                                            <span className="text-[10px] bg-white px-2 py-0.5 border rounded-full text-gray-500 font-bold">
                                                {TagUsage[tag.usage]}
                                            </span>
                                        </div>
                                        <div className="text-xs text-gray-500 font-mono italic">
                                            {tag.usage === TagUsage.Directory ? `dir: /tmp/${tag.value}` : `mod: ...${tag.value}...`}
                                        </div>
                                    </div>
                                </div>
                                <div className="flex gap-1">
                                    <button onClick={() => startEdit(tag)} className="p-1.5 text-gray-400 hover:text-indigo-600 hover:bg-white rounded-lg transition">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                                    </button>
                                    <button onClick={() => deleteTag(tag.id)} className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-white rounded-lg transition">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M3 6h18"/><path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"/><path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2"/></svg>
                                    </button>
                                </div>
                            </div>
                        )}
                    </div>
                ))}
                {tags.length === 0 && <div className="text-center py-10 text-gray-400 italic">No tags defined yet.</div>}
            </div>
        </div>
    );
}
