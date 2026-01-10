import { DownloadStatus } from "./DownloadStatus";

export enum TagUsage {
    Directory = 0,
    Prefix = 1,
    Suffix = 2
}

export type Tag = {
    id: number;
    name: string;
    value: string;
    usage: TagUsage;
    color?: string;
}

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
    tagId?: number;
    tag?: Tag;
};
