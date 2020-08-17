import tl = require("vsts-task-lib/task");
const {JSONPath} = require('jsonpath-plus');

//npm install vsts-task-lib

// Get task parameters
let checkUri: string = tl.getInput("aspNetCoreHealthCheckUri", true);
let checkText: string = tl.getInput("aspNetCoreHealthCheckText", true);
let checkCheck: string = tl.getInput("aspNetCoreHealthCheckCheck", true);

// "Headers": "{\"Content-Type\":\"application/json\"}",

async function run() {
  try {
    //do your actions
    getRequest(checkUri)
      .then(response => {
        console.log(response);
        if (response === checkCheck)
        {
            console.log("response matches the check");
        }
        const result = JSONPath({path: '$.entries.$(aspNetCoreHealthCheckCheck).status', response});
        if (JSONPath.JSONPathClass('$.entries.$(aspNetCoreHealthCheckCheck).status')[0]) === checkText)
        {
            console.log("response matches the check text");
        }
      })
      .catch(error => {
        tl.setResult(tl.TaskResult.Failed, "Request Failed");
        console.log(error);
      });

    tl.setResult(tl.TaskResult.Succeeded, "Success");
  } catch (err) {
    tl.setResult(tl.TaskResult.Failed, err.message);
  }
}

function getRequest(url: string): Promise<any> {
  return new Promise<any>(function(resolve, reject) {
    const request = new XMLHttpRequest();
    request.onload = function() {
      if (this.status === 200) {
        resolve(this.response);
      } else {
        reject(new Error(this.statusText));
      }
    };
    request.onerror = function() {
      reject(new Error("XMLHttpRequest Error: " + this.statusText));
    };
    request.open("GET", url);
    request.send();
  });
}

run();
