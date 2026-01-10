import { useState, useEffect } from "react";
import {
    DndContext,
    closestCenter,
    KeyboardSensor,
    PointerSensor,
    useSensor,
    useSensors,
    DragEndEvent
} from "@dnd-kit/core";
import {
    arrayMove,
    SortableContext,
    sortableKeyboardCoordinates,
    verticalListSortingStrategy,
    useSortable
} from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";

interface OptionSet {
    id: number;
    name: string;
    format: string | null;
    mergeOutputFormat: number | null;
    embedThumbnail: boolean;
    isEnabled: boolean;
    priority: number;
    extractAudio: boolean;
    audioFormat: number | null;
}

const URL = import.meta.env.VITE_API_URL || "";

// Enum mapping for display
const MergeFormats = ["Mp4", "Mkv", "Webm", "Avi"];

interface SortableItemProps {
    opt: OptionSet;
    index: number;
    editingId: number | null;
    startEdit: (opt: OptionSet) => void;
    handleDelete: (id: number) => void;
    handleToggle: (opt: OptionSet) => void;
    getItemContainerClass: (opt: OptionSet) => string;
    tempOption: OptionSet | null;
    setTempOption: (opt: OptionSet) => void;
    saveEdit: () => void;
    setEditingId: (id: number | null) => void;
}

function SortableItem({ 
    opt, index, editingId, startEdit, handleDelete, handleToggle, 
    getItemContainerClass, tempOption, setTempOption, saveEdit, setEditingId 
}: SortableItemProps) {
    const {
        attributes,
        listeners,
        setNodeRef,
        transform,
        transition,
        isDragging
    } = useSortable({ id: opt.id });

    const style = {
        transform: CSS.Transform.toString(transform),
        transition,
        zIndex: isDragging ? 50 : undefined,
        position: 'relative' as const
    };

    return (
        <div ref={setNodeRef} style={style} className={`transition-all duration-300 ${editingId === opt.id ? 'z-10' : ''}`}>
            <div className={`flex flex-col md:flex-row gap-3 p-3 rounded-xl border transition-all ${getItemContainerClass(opt)}`}>
                {/* Drag Handle & Step Indicator */}
                <div className="flex md:flex-col items-center justify-between md:justify-start md:pt-1 gap-2 shrink-0">
                    <div 
                        {...attributes} 
                        {...listeners}
                        className={`w-9 h-9 flex items-center justify-center rounded-xl font-black text-lg cursor-grab active:cursor-grabbing transition-all ${
                            opt.isEnabled ? 'bg-indigo-600 text-white shadow-indigo-100 shadow-lg' : 'bg-gray-300 text-gray-100 shadow-none'
                        }`}
                        title="Drag to reorder"
                    >
                        {index + 1}
                    </div>
                    
                    <div className="flex items-center gap-1 bg-gray-100 p-1 rounded-lg md:hidden">
                        <span className="text-[10px] font-bold text-gray-400 px-1">DRAG ME</span>
                    </div>
                </div>

                {/* Main Content area */}
                <div className="flex-1 min-w-0">
                    <div className="flex flex-wrap items-center justify-between gap-2 mb-2">
                        <div className="flex items-center gap-2">
                            <h4 className={`text-sm font-bold truncate ${opt.isEnabled ? 'text-gray-900' : 'text-gray-500'}`}>
                                {opt.name}
                            </h4>
                            <button 
                                onClick={() => handleToggle(opt)}
                                className={`text-[9px] px-2 py-0.5 rounded-full font-black uppercase tracking-widest transition-all cursor-pointer border ${
                                    opt.isEnabled 
                                    ? 'bg-emerald-50 text-emerald-700 border-emerald-100 hover:bg-emerald-100' 
                                    : 'bg-gray-200 text-gray-600 border-gray-300 hover:bg-gray-300'
                                }`}
                            >
                                {opt.isEnabled ? 'Active' : 'Off'}
                            </button>
                        </div>
                        
                        <div className="flex items-center gap-1">
                            {editingId === opt.id ? (
                                <button onClick={() => setEditingId(null)} className="text-[10px] font-bold text-gray-400 hover:text-gray-600 px-2 py-1">Discard</button>
                            ) : (
                                <>
                                    <button 
                                        onClick={() => startEdit(opt)} 
                                        className="p-1.5 text-gray-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-all cursor-pointer"
                                        title="Edit"
                                    >
                                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                                    </button>
                                    <button 
                                        onClick={() => handleDelete(opt.id)} 
                                        className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-all cursor-pointer"
                                        title="Delete"
                                    >
                                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M3 6h18"/><path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"/><path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2"/></svg>
                                    </button>
                                </>
                            )}
                        </div>
                    </div>

                    {editingId === opt.id && tempOption ? (
                        <div className="space-y-3 animate-in fade-in slide-in-from-top-1 duration-200">
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                                <div className="flex flex-col gap-1">
                                    <label htmlFor={`name-${opt.id}`} className="text-[9px] font-black text-slate-500 uppercase tracking-widest ml-1">Label</label>
                                    <input 
                                        id={`name-${opt.id}`}
                                        className="w-full text-xs font-bold p-2 border-2 border-slate-100 bg-slate-50 rounded-lg focus:border-indigo-500 focus:bg-white focus:outline-none transition-all shadow-inner" 
                                        value={tempOption.name} 
                                        onChange={e => setTempOption({...tempOption, name: e.target.value})}
                                    />
                                </div>
                                <div className="flex flex-col gap-1">
                                    <label htmlFor={`format-${opt.id}`} className="text-[9px] font-black text-slate-500 uppercase tracking-widest ml-1">yt-dlp Format</label>
                                    <input 
                                        id={`format-${opt.id}`}
                                        className="w-full text-xs p-2 border-2 border-slate-100 bg-slate-50 rounded-lg font-mono font-bold focus:border-indigo-500 focus:bg-white focus:outline-none transition-all shadow-inner" 
                                        placeholder="e.g. bestvideo+bestaudio/best"
                                        value={tempOption.format || ""} 
                                        onChange={e => setTempOption({...tempOption, format: e.target.value})}
                                    />
                                </div>
                            </div>

                            <div className="flex flex-wrap items-center gap-4 bg-indigo-50/50 p-3 rounded-lg border border-dashed border-indigo-100">
                                <div className="flex items-center gap-4">
                                    <label className="flex items-center gap-2 text-xs font-bold text-slate-700 cursor-pointer group">
                                        <div className="relative flex items-center">
                                            <input 
                                                type="checkbox" 
                                                className="peer sr-only"
                                                checked={tempOption.embedThumbnail} 
                                                onChange={e => setTempOption({...tempOption, embedThumbnail: e.target.checked})}
                                            />
                                            <div className="w-8 h-4 bg-gray-200 rounded-full peer-checked:bg-indigo-600 transition-colors after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-3 after:w-3 after:shadow-sm after:transition-all peer-checked:after:translate-x-4"></div>
                                        </div>
                                        Thumbnail
                                    </label>
                                    
                                    <label className="flex items-center gap-2 text-xs font-bold text-slate-700 cursor-pointer group">
                                        <div className="relative flex items-center">
                                            <input 
                                                type="checkbox" 
                                                className="peer sr-only"
                                                checked={tempOption.extractAudio} 
                                                onChange={e => setTempOption({...tempOption, extractAudio: e.target.checked})}
                                            />
                                            <div className="w-8 h-4 bg-gray-200 rounded-full peer-checked:bg-orange-600 transition-colors after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-3 after:w-3 after:shadow-sm after:transition-all peer-checked:after:translate-x-4"></div>
                                        </div>
                                        MP3
                                    </label>
                                </div>

                                <div className="flex items-center gap-2 ml-auto">
                                    <span className="text-[10px] font-black text-indigo-400 uppercase tracking-widest">Merge:</span>
                                    <select 
                                        className="text-xs px-2 py-1 border rounded-lg bg-white font-bold text-indigo-700 focus:border-indigo-500 outline-none shadow-sm cursor-pointer"
                                        value={tempOption.mergeOutputFormat || 0}
                                        onChange={e => setTempOption({...tempOption, mergeOutputFormat: Number.parseInt(e.target.value)})}
                                    >
                                        {MergeFormats.map((f, i) => <option key={f} value={i}>{f}</option>)}
                                    </select>
                                </div>
                            </div>

                            <div className="flex justify-end gap-2">
                                <button 
                                    onClick={saveEdit} 
                                    className="px-4 py-1.5 text-xs font-black bg-indigo-600 text-white rounded-lg shadow-md hover:bg-indigo-700 transition-all cursor-pointer"
                                >
                                    Save Strategy
                                </button>
                            </div>
                        </div>
                    ) : (
                        <div className="flex flex-wrap items-center gap-2">
                            <div className="flex-1 min-w-[200px] flex items-center gap-2 bg-gray-50/80 p-2 rounded-xl border border-gray-100 shadow-inner group-hover:bg-white transition-colors">
                                <div className="w-7 h-7 flex items-center justify-center bg-gray-200/50 rounded-lg text-gray-500 shrink-0">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round"><polyline points="4 17 10 11 4 5"/><line x1="12" y1="19" x2="20" y2="19"/></svg>
                                </div>
                                <code className="text-[10px] text-indigo-700 font-mono font-bold truncate">
                                    {opt.format || "automatic selection"}
                                </code>
                            </div>
                            <div className="flex gap-1.5">
                                {opt.embedThumbnail && (
                                    <span className="inline-flex items-center gap-1 text-[9px] font-black uppercase bg-indigo-50 text-indigo-700 px-2 py-1 rounded-lg border border-indigo-100">
                                        🖼️ Thumb
                                    </span>
                                )}
                                {opt.extractAudio && (
                                    <span className="inline-flex items-center gap-1 text-[9px] font-black uppercase bg-orange-50 text-orange-700 px-2 py-1 rounded-lg border border-orange-100">
                                        🎵 Audio
                                    </span>
                                )}
                                <span className="inline-flex items-center text-[9px] font-black uppercase bg-slate-100 text-slate-700 px-2 py-1 rounded-lg border border-slate-200">
                                    📦 {MergeFormats[opt.mergeOutputFormat || 0]}
                                </span>
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}

export function OptionSetManager() {
    const [options, setOptions] = useState<OptionSet[]>([]);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [tempOption, setTempOption] = useState<OptionSet | null>(null);
    const [loading, setLoading] = useState(false);
    const [hasUnsavedPriorities, setHasUnsavedPriorities] = useState(false);

    const sensors = useSensors(
        useSensor(PointerSensor),
        useSensor(KeyboardSensor, {
            coordinateGetter: sortableKeyboardCoordinates,
        })
    );

    useEffect(() => {
        fetchOptions();
    }, []);

    const fetchOptions = async () => {
        try {
            const res = await fetch(`${URL}/optionsets`);
            const data = await res.json();
            setOptions(data.sort((a: OptionSet, b: OptionSet) => a.priority - b.priority));
            setHasUnsavedPriorities(false);
        } catch (e) {
            console.error("Failed to fetch options", e);
        }
    };

    const handleToggle = async (opt: OptionSet) => {
        const updated = { ...opt, isEnabled: !opt.isEnabled };
        try {
             await fetch(`${URL}/optionsets/${opt.id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(updated)
            });
            fetchOptions();
        } catch (e) {
            console.error(e);
        }
    };

    const handleDragEnd = (event: DragEndEvent) => {
        const { active, over } = event;

        if (over && active.id !== over.id) {
            setOptions((items) => {
                const oldIndex = items.findIndex((i) => i.id === active.id);
                const newIndex = items.findIndex((i) => i.id === over.id);
                const newItems = arrayMove(items, oldIndex, newIndex);
                
                // Update priorities based on new indices
                return newItems.map((item, index) => ({
                    ...item,
                    priority: index
                }));
            });
            setHasUnsavedPriorities(true);
        }
    };

    const savePriorities = async () => {
        setLoading(true);
        try {
            await fetch(`${URL}/optionsets/priorities`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    priorities: options.map(o => ({ id: o.id, priority: o.priority }))
                })
            });
            setHasUnsavedPriorities(false);
        } catch (e) {
            console.error(e);
            alert("Failed to save priorities");
        } finally {
            setLoading(false);
        }
    };

    const startEdit = (opt: OptionSet) => {
        setEditingId(opt.id);
        setTempOption({ ...opt });
    };

    const saveEdit = async () => {
        if (!tempOption) return;
        try {
            await fetch(`${URL}/optionsets/${tempOption.id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(tempOption)
            });
            setEditingId(null);
            fetchOptions();
        } catch (e) {
            console.error(e);
        }
    };

    const handleDelete = async (id: number) => {
        if (!confirm("Delete this strategy?")) return;
        try {
            await fetch(`${URL}/optionsets/${id}`, { method: 'DELETE' });
            fetchOptions();
        } catch (e) {
            console.error(e);
        }
    };

    const addNew = async () => {
        const newOpt = {
            name: "New Strategy",
            format: "",
            isEnabled: true,
            priority: options.length,
            embedThumbnail: true,
            extractAudio: false
        };
        try {
            await fetch(`${URL}/optionsets`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(newOpt)
            });
            fetchOptions();
        } catch (e) {
            console.error(e);
        }
    };

    const getOrderButtonText = () => {
        if (loading) return "Saving...";
        return hasUnsavedPriorities ? "Save Order" : "Order Saved";
    };

    const getItemContainerClass = (opt: OptionSet) => {
        if (editingId === opt.id) {
            return 'bg-white shadow-2xl ring-2 ring-indigo-500 border-transparent';
        }
        if (opt.isEnabled) {
            return 'bg-white shadow-sm border-gray-200 hover:border-indigo-300 hover:shadow-md';
        }
        return 'bg-gray-100/50 border-gray-200 opacity-60 grayscale-[0.5]';
    };

    return (
        <div className="bg-white rounded-xl shadow-lg border border-gray-200 overflow-hidden flex flex-col">
            {/* Header section */}
            <div className="p-4 bg-white border-b border-gray-100">
                <div className="flex justify-between items-start mb-4">
                    <div>
                        <h3 className="text-lg font-bold text-gray-900 flex items-center gap-2">
                            <div className="flex items-center justify-center w-8 h-8 bg-indigo-600 text-white rounded-lg shadow-md ring-2 ring-indigo-50">
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><path d="m12 19-7-7 7-7"/><path d="M19 12H5"/></svg>
                            </div>
                            Execution Pipeline
                        </h3>
                    </div>
                    <div className="flex gap-2">
                        <button 
                            onClick={addNew} 
                            className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-bold text-indigo-700 bg-indigo-50 rounded-lg hover:bg-indigo-100 focus:outline-none focus:ring-2 focus:ring-indigo-500 transition-all cursor-pointer border border-indigo-100"
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><path d="M5 12h14"/><path d="M12 5v14"/></svg>
                            Add Step
                        </button>
                        <button 
                            onClick={savePriorities} 
                            disabled={loading || !hasUnsavedPriorities}
                            className={`inline-flex items-center px-3 py-1.5 text-xs font-bold rounded-lg shadow-md transition-all cursor-pointer ${
                                hasUnsavedPriorities 
                                ? 'bg-indigo-600 text-white hover:bg-indigo-700' 
                                : 'bg-gray-100 text-gray-400 cursor-not-allowed border border-gray-200 shadow-none'
                            }`}
                        >
                            {getOrderButtonText()}
                        </button>
                    </div>
                </div>
                
                <div className="bg-slate-50 border border-slate-200 rounded-lg p-3 text-xs text-slate-700">
                    <div className="flex gap-3">
                        <div className="shrink-0 w-6 h-6 flex items-center justify-center bg-indigo-100 text-indigo-600 rounded-full text-sm">💡</div>
                        <div className="space-y-2">
                            <p className="font-bold text-slate-900">Priority-Based Fallback System</p>
                            <div className="leading-relaxed opacity-90">
                                <p>Drag and drop items to reorder. Processes sequentially: starts with <strong>Step 1</strong> and only proceeds the next if yt-dlp fails.</p>
                                <div className="mt-2 space-y-1 bg-white/50 p-2 rounded border border-slate-200">
                                    <p className="font-semibold text-indigo-700">How to configure:</p>
                                    <ul className="list-disc pl-4 space-y-1">
                                        <li><strong>Format:</strong> Use <code className="bg-white px-1">bv+ba/b</code> for best quality, <code className="bg-white px-1">b[height{"<="}1080]</code> for limits, or <code className="bg-white px-1">ba</code> for audio only.</li>
                                        <li><strong>Thumbnail:</strong> Injects the video cover art into the file metadata.</li>
                                        <li><strong>MP3:</strong> Extracts and converts audio to MP3 after download.</li>
                                        <li><strong>Merge:</strong> The final container format (MP4 is most compatible).</li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            
            {/* List entries */}
            <div className="p-4 bg-gray-50/50">
                <DndContext 
                    sensors={sensors}
                    collisionDetection={closestCenter}
                    onDragEnd={handleDragEnd}
                >
                    <SortableContext 
                        items={options.map(o => o.id)}
                        strategy={verticalListSortingStrategy}
                    >
                        <div className="space-y-3">
                            {options.map((opt, index) => (
                                <SortableItem 
                                    key={opt.id}
                                    opt={opt}
                                    index={index}
                                    editingId={editingId}
                                    startEdit={startEdit}
                                    handleDelete={handleDelete}
                                    handleToggle={handleToggle}
                                    getItemContainerClass={getItemContainerClass}
                                    tempOption={tempOption}
                                    setTempOption={(o) => setTempOption(o)}
                                    saveEdit={saveEdit}
                                    setEditingId={setEditingId}
                                />
                            ))}
                        </div>
                    </SortableContext>
                </DndContext>
            </div>
        </div>
    );
}

// Remove old move functions if they are exported or keep them as internal helpers
