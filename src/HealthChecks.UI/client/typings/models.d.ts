export interface HttpRequest {
    method: string;
    url: string;
    contentType?: string;
    content?: any;
}
export interface Liveness {
    livenessName: string;
    onStateFrom: string;
    lastExecuted: string;
    status: string;
    livenessResult: string;
    discoveryService: string;
    checks : Array<Check>;
}

export interface LivenessResult {
    checks : Array<Check> | string;    
}

export interface Check {
    name: string;
    message: string;
    exception: string;
    elapsed: string;
    run: boolean;
    path: string,
    isHealthy: boolean
}

interface WebHook {
    name: string;
    uri: string;
    payload: string;
}