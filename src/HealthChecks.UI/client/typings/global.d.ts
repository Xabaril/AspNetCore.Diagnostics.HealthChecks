type Nullable<T> = T | null;

interface ClientOptions {
    defaultPollingIntervalSeconds: number;
    minimumPollingIntervalSeconds: number;
    hidePollingIntervalControl: boolean;
}

declare const clientOptions: Readonly<ClientOptions>;
