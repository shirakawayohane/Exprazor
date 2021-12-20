import { DOMCommand } from "./DOMCommands";

declare var __DEV__ : any;

export type FromServerCommand = HandleCommands | InvokeClientSideVoid;

export type HandleCommands = [_ : 0, commands : DOMCommand[]];
export function isHandleCommands(value : any) : value is HandleCommands {
    return value[0] === 0;
}

export type InvokeClientSideVoid = [_ : 1, functionName : string, ...args: (object | undefined | null)[] | undefined | null]
export function isInvokeClientSideVoid(value : any) : value is InvokeClientSideVoid {
    return value[0] === 1;
}