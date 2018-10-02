import { HttpError } from "./httpError";
import { HttpResponse } from "./httpResponse";
import { HttpRequest } from "../typings/models";

export class HttpClient {
    sendRequest(request: HttpRequest) {
        
        return new Promise((resolve, reject) => {
            const xhr = new XMLHttpRequest();
            xhr.open(request.method, request.url, true);

            xhr.setRequestHeader("X-Request-Client", "XMLHttpClient");
            xhr.setRequestHeader("Content-type", request.contentType || "application/json");

            xhr.onload = () => {
                if (xhr.status >= 200 && xhr.status < 300) {
                    resolve(new HttpResponse(xhr));
                } else {
                    reject(new HttpError(xhr));
                }
            }
            xhr.onerror = () => {
                reject(new HttpError(xhr));
            }

            xhr.ontimeout = () => {
                reject(new HttpError(xhr));
            }
            xhr.send(request.content || "");
        });
    }
}