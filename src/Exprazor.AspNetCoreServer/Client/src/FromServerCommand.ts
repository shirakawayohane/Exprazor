import { DOMCommand } from "./DOMCommands";

declare var __DEV__ : any;

// This type will be union type later.
export type FromServerCommand = HandleCommands;

export type HandleCommands = [_ : 0, commands : DOMCommand[]];
export function isHandleCommands(value : any) : value is HandleCommands {
    return value[0] === 0;
}