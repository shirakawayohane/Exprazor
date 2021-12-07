import signalR from "@microsoft/signalr";
import { DOMCommand, Id, isCreateTextNode, isRemoveAttribute, isRemoveCallback, isSetBooleanAttribute, isSetNumberAttribute, isSetStringAttribute, isSetVoidCallback } from "./DOMCommands";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/exprazorhub")
    .withAutomaticReconnect()
    .build();

const elements: { [id:Id] : HTMLElement } = {};
const texts: { [id: Id] : Text } = {};

connection.on("handleCommands", (commands) => {
    commands.forEach(cmd => {
        if(isSetStringAttribute(cmd)) {
            elements[cmd.Id].setAttribute(cmd.Key, cmd.Value);
        } else if(isSetNumberAttribute(cmd)) {
            elements[cmd.Id].setAttribute(cmd.Key, cmd.Value.toString());
        } else if(isSetBooleanAttribute(cmd)) {
            if(cmd.Value) {
                elements[cmd.Id].setAttribute(cmd.Key, "");
            } else {
                elements[cmd.Id]
            }
        } else if(isRemoveAttribute(cmd)) {
            elements[cmd.Id].removeAttribute(cmd.Key);
        } else if(isSetVoidCallback(cmd)) {
            switch(cmd.Key) {
                // Only support void callback for now.
                default:
                    elements[cmd.Id][cmd.Key] = connection.send("InvokeVoid", cmd.Id, cmd.Key);
                    break;
            }
        } else if(isRemoveCallback(cmd)) {
            elements[cmd.Id][cmd.Key] = null;
        } else if(isCreateTextNode(cmd)) {
            texts[cmd.Id] = document.createTextNode(cmd.Text);
        } else if() //WIP
    });
});