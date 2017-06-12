var request = require('request');
var url = require('url');

function send(uri, headers, body, pipeTo) {
    return new Promise((resolve, reject) => {
        var requestOpts = {
            uri: uri,
            headers: headers,
            json: true,
            body: body
        };

        // Update the host header to the sub-bot host name
        requestOpts.headers.host = url.parse(requestOpts.uri).host;

        // Forward request to sub-bot
        request.post(requestOpts, (err, resp, body) => {
            if (err) {
                reject(err);
            } else {
                resolve(resp);
            }
        }).pipe(pipeTo); 
    });
}

module.exports.send = send;