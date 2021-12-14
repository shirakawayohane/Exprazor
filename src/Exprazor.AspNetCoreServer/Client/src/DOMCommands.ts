import { getIsDevelopment } from "./Utils";

declare var __DEV__ : any;

export type Id = number;

export type DOMCommand = 
    SetStringAttribute  |
    SetNumberAttribute  |
    SetBooleanAttribute |
    RemoveAttribute     |
    SetTextNodeValue    |
    CreateTextNode      |
    CreateElement       |
    AppendChild         |
    InsertBefore        |
    RemoveChild         |
    RemoveCallback      |
    SetVoidCallback     |
    SetStringCallback
    ;

export type SetStringAttribute = [_ : 0, id : Id, key : string, value : string];
export function isSetStringAttribute(value : any) : value is SetStringAttribute {
    return value[0] === 0;
}
export type SetNumberAttribute = [_ : 1, id : Id, key : string, value : number];
export function isSetNumberAttribute(value : any) : value is SetNumberAttribute {
    return value[0] === 1;
}

export type SetBooleanAttribute = [_ : 2, id : Id, key : string, value : boolean];
export function isSetBooleanAttribute(value : any) : value is SetBooleanAttribute {
    return value[0] === 2;
}

export type RemoveAttribute = [_ : 3, id : Id, key : string];
export function isRemoveAttribute(value : any) : value is RemoveAttribute {
    return value[0] === 3;
}

export type SetTextNodeValue = [_ : 4, id : Id, text : string];
export function isSetTextNodeValue(value : any) : value is SetTextNodeValue {
    return value[0] === 4;
}

export type CreateTextNode = [_ : 10, id : Id, text : string];
export function isCreateTextNode(value : any) : value is CreateTextNode {
    return value[0] === 10;
}

export type CreateElement = [_ : 11, id : Id, tag : string];
export function isCreateElement(value : any) : value is CreateElement {
    return value[0] === 11;
}

export type AppendChild = [_  : 20, parentId : Id, newId : Id];
export function isAppendChild(value : any) : value is AppendChild {
    return value[0] === 20;
}

export type InsertBefore = [_ : 21, parentId: Id, newId: Id, beforeId: Id];
export function isInsertBefore(value : any) : value is InsertBefore {
    return value[0] === 21;
}

export type RemoveChild = [_ : 22, parentId: Id, childId : Id];
export function isRemoveChild(value : any) : value is RemoveChild {
    return value[0] === 22;
}

export type RemoveCallback = [_ : 30, id : Id, key : string];
export function isRemoveCallback(value : any) : value is RemoveCallback {
    return value[0] === 30;
}

export type SetVoidCallback = [_ : 31, id : Id, key : string];
export function isSetVoidCallback(value : any) : value is SetVoidCallback {
    return value[0] === 31;
}

export type SetStringCallback = [_ : 32, id : Id, key : string];
export function isSetStringCallback(value : any) : value is SetStringCallback {
    return value[0] === 32;
}

export function getKeyedObject(cmd : DOMCommand) : object {
    if(isSetStringAttribute(cmd)) {
        return {
            name : "SetStringAttribute", id : cmd[1], key : cmd[2], value : cmd[3]
        };
    } else if(isSetNumberAttribute(cmd)) {
        return {
            name : "SetNumberAttribute", id : cmd[1], key : cmd[2], value : cmd[3]
        };
    } else if(isSetBooleanAttribute(cmd)) {
        return {
            name : "SetBooleanAttribute", id : cmd[1], key : cmd[2], value : cmd[3]
        };
    } else if(isRemoveAttribute(cmd)) {
        return {
            name : "RemoveAttribute", id : cmd[1], key : cmd[2]
        };
    } else if(isSetTextNodeValue(cmd)) {
        return {
            name : "SetTextNodeValue", id : cmd[1], text : cmd[2]
        }
    } else if(isCreateTextNode(cmd)) {
        return {
            name : "CreateTextNode", id : cmd[1], text : cmd[2]
        }
    } else if(isCreateElement(cmd)) {
        return {
            name : "CreateElement", id : cmd[1], tag : cmd[2]
        }
    } else if(isAppendChild(cmd)) {
        return {
            name : "AppendChild", parentId : cmd[1], newId : cmd[2]
        }
    } else if(isInsertBefore(cmd)) {
        return {
            name : "InsertBefore", parentId: cmd[1], newId : cmd[2], beforeId : cmd[3]
        }
    } else if(isRemoveChild(cmd)) {
        return {
            name : "RemoveChild" , parentId : cmd[1], childId : cmd[2]
        }
    } else if(isRemoveCallback(cmd)) {
        return {
            name : "RemoveCallback", id : cmd[1], key : cmd[2]
        }
    } else if(isSetVoidCallback(cmd)) {
        return {
            name : "SetVoidCallback", id : cmd[1], key : cmd[2]
        }
    } else if(isSetStringCallback(cmd)) {
        return {
            name : "SetStringCallback", id : cmd[1], key : cmd[2]
        }
    }
}