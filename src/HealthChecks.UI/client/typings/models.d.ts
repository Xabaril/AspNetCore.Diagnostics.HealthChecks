export interface HttpRequest {
    method: string;
    url: string;
    contentType?: string;
    content?: any;
}
export interface Liveness {
    name: string;
    onStateFrom: string;
    lastExecuted: string;
    status: string;
    livenessResult: string;
    discoveryService: string;
    entries : Array<Check>;
}

export interface LivenessResult {
    checks : Array<Check> | string;    
}

export interface Check {
    name: string;
    status: string;
    description: string;
    duration: string;    
}

interface WebHook {
    name: string;
    uri: string;
    payload: string;
}