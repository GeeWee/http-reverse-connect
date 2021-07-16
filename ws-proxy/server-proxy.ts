import WebSocket from 'ws';
import axios, {AxiosRequestConfig} from "axios";

const ws = new WebSocket('ws://127.0.0.1:8081');

ws.on('open', function open() {
    console.log("Websocket connection opened")
});

ws.on('message', async function incoming(message: string) {
    // console.log('received: %s', message);

    // todo validate
    const rpcCall = JSON.parse(message);
    
    console.log(rpcCall, typeof rpcCall);

    const axiosConfig: AxiosRequestConfig = {
        method: rpcCall.Method,
        url: rpcCall.Url,
        responseType: "json",
        data: rpcCall.Body
    };
    console.log(axiosConfig);
    const response = await axios(axiosConfig);
    
    
    const rpcResponse: RpcResponse = {
        url: rpcCall.Url,
        body: response.data,
        method: rpcCall.Method,
        statusCode: response.status,
        headers: response.headers
    }
    

    ws.send(JSON.stringify(rpcResponse));

});

interface RpcResponse {
    url: string,
    method: "GET" | "POST" | "PUT" | "PATCH"
    statusCode: number,
    body: any
    headers: Record<string, string>
}

interface RpcCall {
    Url: string,
    Method: "GET" | "POST" | "PUT" | "PATCH",
    Headers: Record<string, string>,
    // TODO body could be array too
    Body: Record<string, any> | Array<any>,
}

// todo return rpccall