import { DownloadStatus } from "./DownloadStatus";

export type DownloadItem = {
    readonly id: number;
    readonly url: string;
    thumbnail?: string;
    title?: string;
    status: DownloadStatus;
    progress: number;
    totalSize?: number;
    speed?: string;
    eta?: string;
    errorMessage?: string;
    created: Date;
    started?: Date;
    finished?: Date;
    readonly later: boolean;
};
