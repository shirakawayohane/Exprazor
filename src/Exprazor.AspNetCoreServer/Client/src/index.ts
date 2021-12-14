import { isHandleCommands } from "./FromServerCommand";
import { DOMCommand, getKeyedObject, Id, isAppendChild, isCreateElement, isCreateTextNode, isInsertBefore, isRemoveAttribute, isRemoveCallback, isRemoveChild, isSetBooleanAttribute, isSetNumberAttribute, isSetStringAttribute, isSetTextNodeValue, isSetVoidCallback, toString } from "./DOMCommands";
import * as msgpack from "@msgpack/msgpack";
declare var __DEV__ : any;

const idToElement: Map<Id, Node> = new Map();
const elementToId : Map<Node, Id> = new Map();

const MOUNT_ID = -1;
const rootNode = document.querySelector("#root");
if(rootNode) {
    idToElement.set(MOUNT_ID, rootNode);
    elementToId.set(rootNode, MOUNT_ID);

    const location = window.location;
    let hubUri = `${location.protocol === "https:" ? "wss:" : "ws:"}//${location.host}${location.pathname}`;
    const socket = new WebSocket(hubUri);

    socket.addEventListener("open", event => {
        console.log("open!");
        socket.send(msgpack.encode([0]));
    });
    socket.addEventListener("message", event => {
        event.data.arrayBuffer().then(buffer => {
            const data = msgpack.decode(buffer);
            if(isHandleCommands(data)) {
                data[1].forEach(cmd => {
                    __DEV__ && console.log(getKeyedObject(cmd));
                    __DEV__ && console.log(idToElement);
                    if (isSetStringAttribute(cmd)) {
                        (idToElement.get(cmd[1]) as HTMLElement).setAttribute(cmd[2], cmd[3]);
                    } else if (isSetNumberAttribute(cmd)) {
                        (idToElement.get(cmd[1]) as HTMLElement).setAttribute(cmd[2], cmd[3].toString());
                    } else if (isSetBooleanAttribute(cmd)) {
                        if (cmd[3]) {
                            (idToElement.get(cmd[1]) as HTMLElement).setAttribute(cmd[2], "");
                        } else {
                            (idToElement.get(cmd[1]) as HTMLElement).removeAttribute(cmd[2]);
                        }
                    } else if (isRemoveAttribute(cmd)) {
                        (idToElement.get(cmd[1]) as HTMLElement).removeAttribute(cmd[2]);
                    } else if (isSetVoidCallback(cmd)) {
                        idToElement.get(cmd[1])[cmd[2]] = () => socket.send(msgpack.encode([1, cmd[1], cmd[2]]));
                    } else if (isRemoveCallback(cmd)) {
                        idToElement.get(cmd[1])[cmd[2]] = null;
                    } else if (isCreateTextNode(cmd)) {
                        const newNode = document.createTextNode(cmd[2]);
                        idToElement.set(cmd[1], newNode);
                        elementToId.set(newNode, cmd[1]);
                    } else if (isCreateElement(cmd)) {
                        const newNode = document.createElement(cmd[2]);
                        idToElement.set(cmd[1], newNode);
                        elementToId.set(newNode, cmd[1]);
                    } else if (isAppendChild(cmd)) {
                        idToElement.get(cmd[1]).appendChild(idToElement.get(cmd[2]));
                    } else if(isSetTextNodeValue(cmd)) {
                        (idToElement.get(cmd[1]) as Text).data = cmd[2];
                    } else if(isInsertBefore(cmd)) {
                        idToElement.get(cmd[1]).insertBefore(idToElement.get(cmd[2]), idToElement.get(cmd[3]));
                    } else if(isRemoveChild(cmd)) {
                        const childToRemove = idToElement.get(cmd[2]);
                        if(childToRemove instanceof HTMLElement) {
                            // remove all child from map to prevent memory leak.
                            for(const child of childToRemove.querySelectorAll("*")) {
                                var id = elementToId.get(child);
                                idToElement.delete(id);
                                elementToId.delete(child);
                            }
                        }
                        idToElement[cmd[1]].removeChild(childToRemove);
                    }
                });
            }
        });
    });
}