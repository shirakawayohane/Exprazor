import { DOMCommand } from "./DOMCommands";

declare var __DEV__ : any;

// This type will be union type later.
export type ClientCommand = HandleCommands;

export type HandleCommands = {
    Commands: DOMCommand[]
}

function isHandleCommandsProd(value: any) : value is HandleCommands {
    return value.Type === 0;
}

function isHandleCommandsDev(value : any) : value is HandleCommands {
    return value.Type === "HandleCommands";
}

export const isHandleCommands : (value : any) => value is HandleCommands = __DEV__ ? isHandleCommandsDev : isHandleCommandsProd;