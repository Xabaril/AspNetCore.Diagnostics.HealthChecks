import { HttpClient } from "./http/httpClient";

export class HealthChecksClient {
    private _http: HttpClient;
    private _pollingInterval: Nullable<number>;

    constructor(private endpoint: string) {
        this.endpoint = endpoint;
        this._http = new HttpClient();
        this._pollingInterval = null;
    }
    getData(): Promise<any> {
        return this._http.sendRequest({
            url: this.endpoint,
            method: "get"
        });
    }

    startPolling(interval: undefined | number, onTimeElapsedCallback: () => void) {
        this.stopPolling();
        this._pollingInterval = setInterval(onTimeElapsedCallback, interval);
    }

    stopPolling() {
        this._pollingInterval && clearInterval(this._pollingInterval);
    }
}