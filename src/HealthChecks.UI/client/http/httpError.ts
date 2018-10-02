export class HttpError {
    
    private status: number;
    private response : any;
            
    constructor(xmlHttpRequest: XMLHttpRequest) {
        var {response, status} =  xmlHttpRequest;
        this.response = response;
        this.status = status;        
    }
}
