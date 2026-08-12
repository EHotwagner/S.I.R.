
import { HubConnectionBuilder } from "@microsoft/signalr";

export function build(url, accessToken) {
    return (new HubConnectionBuilder()).withUrl(url, {
        accessTokenFactory: () => accessToken,
        transport: 4,
    }).withAutomaticReconnect().build();
}

