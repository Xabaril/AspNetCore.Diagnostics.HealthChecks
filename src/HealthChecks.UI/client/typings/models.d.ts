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
    history: Array<ExecutionHistory>;
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

export interface ExecutionHistory {
    name: string;
    description: string;
    id: number;
    status: string;
    on: string;
}

interface WebHook {
    name: string;
    host: string;
    payload: string;
}