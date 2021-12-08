import * as signalR from "@microsoft/signalr";
import { DOMCommand, Id, isAppendChild, isCreateElement, isCreateTextNode, isInsertBefore, isRemoveAttribute, isRemoveCallback, isRemoveChild, isSetBooleanAttribute, isSetNumberAttribute, isSetStringAttribute, isSetTextNodeValue, isSetVoidCallback } from "./DOMCommands";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/exprazorhub")
    .withAutomaticReconnect()
    .build();

const idToElement: Map<Id, Node> = new Map();
const elementToId : Map<Node, Id> = new Map();

const MOUNT_ID = -1;
idToElement[MOUNT_ID] = document.querySelector("body");
elementToId[idToElement[MOUNT_ID]] = MOUNT_ID;

connection.on("handleCommands", (commands) => {
    commands.forEach(cmd => {
        if (isSetStringAttribute(cmd)) {
            (idToElement.get(cmd.Id) as HTMLElement).setAttribute(cmd.Key, cmd.Value);
        } else if (isSetNumberAttribute(cmd)) {
            (idToElement.get(cmd.Id) as HTMLElement).setAttribute(cmd.Key, cmd.Value.toString());
        } else if (isSetBooleanAttribute(cmd)) {
            if (cmd.Value) {
                (idToElement.get(cmd.Id) as HTMLElement).setAttribute(cmd.Key, "");
            } else {
                (idToElement.get(cmd.Id) as HTMLElement).removeAttribute(cmd.Key);
            }
        } else if (isRemoveAttribute(cmd)) {
            (idToElement.get(cmd.Id) as HTMLElement).removeAttribute(cmd.Key);
        } else if (isSetVoidCallback(cmd)) {
            switch (cmd.Key) {
                // Only support void callback for now.
                default:
                    idToElement.get(cmd.Id)[cmd.Key] = connection.send("InvokeVoid", cmd.Id, cmd.Key);
                    break;
            }
        } else if (isRemoveCallback(cmd)) {
            idToElement.get(cmd.Id)[cmd.Key] = null;
        } else if (isCreateTextNode(cmd)) {
            const newNode = document.createTextNode(cmd.Text);
            idToElement.set(cmd.Id, newNode);
            elementToId.set(newNode, cmd.Id);
        } else if (isCreateElement(cmd)) {
            const newNode = document.createElement(cmd.Tag);
            idToElement.set(cmd.Id, newNode);
            elementToId.set(newNode, cmd.Id);
        } else if (isAppendChild(cmd)) {
            idToElement.get(cmd.ParentId).appendChild(idToElement.get(cmd.NewId));
        } else if(isSetTextNodeValue(cmd)) {
            (idToElement.get(cmd.Id) as Text).textContent = cmd.Text;
        } else if(isInsertBefore(cmd)) {
            idToElement.get(cmd.ParentId).insertBefore(idToElement.get(cmd.NewId), idToElement.get(cmd.BeforeId));
        } else if(isRemoveChild(cmd)) {
            const childToRemove = idToElement.get(cmd.ChildId);
            if(childToRemove instanceof HTMLElement) {
                // remove all child from map to prevent memory leak.
                for(const child of childToRemove.querySelectorAll("*")) {
                    var id = elementToId.get(child);
                    idToElement.delete(id);
                    elementToId.delete(child);
                }
            }
            idToElement[cmd.ParentId].removeChild(childToRemove);
        }
    });
});