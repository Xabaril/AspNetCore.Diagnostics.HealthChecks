export class HttpResponse {

    private status : number;
    private statusText : string;
    private response: any;
    private data: any;

    constructor(xmlHttpRequest : XMLHttpRequest) {        
        const {status, statusText, response, responseText} =  xmlHttpRequest;
        this.status = status;
        this.statusText = statusText;
        this.response = response || responseText;
        this.data = JSON.parse(this.response);
    }
}