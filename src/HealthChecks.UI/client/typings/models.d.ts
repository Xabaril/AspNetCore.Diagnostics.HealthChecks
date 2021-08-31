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
  entries: Array<Check>;
  history: Array<ExecutionHistory>;
  group: string;
}

export interface CustomGrouping {
  executions: Liveness[];
  lastExecuted: string;
  name: string;
  onStateFrom: string;
  status: string;
}

export interface LivenessResult {
  checks: Array<Check> | string;
}

export interface Check {
  name: string;
  status: string;
  description: string;
  duration: string;
  tags: string[];
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

export interface UIApiSettings {
  pollingInterval: number;
  headerText: string;
}
