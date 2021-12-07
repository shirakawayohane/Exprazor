export enum CommandType { 
    SetStringAttribute = 1,
    SetNumberAttribute = 21
}


export type Id = number;

export type DOMCommand = 
    SetStringAttribute  |
    SetNumberAttribute  |
    SetBooleanAttribute |
    RemoveAttribute     |
    SetVoidCallback     |
    RemoveCallback      |
    CreateTextNode      |
    CreateElement       |
    AppendChild         |
    SetTextNodeValue    |
    InsertBefore        |
    RemoveChild
    ;

export type SetStringAttribute = {
    Id: Id,
    Key: string,
    Value: string
}

export type SetNumberAttribute = {
    Id: Id,
    Key: string,
    Value: number
}

export type SetBooleanAttribute = {
    Id:Id,
    Key: string,
    Value: boolean
}

export type RemoveAttribute = {
    Id: Id,
    Key: string
}

export type SetVoidCallback = {
    Id: Id,
    Key: string,
}

export type RemoveCallback = {
    Id: Id,
    Key: string,
}

export type CreateTextNode = {
    Id: Id,
    Text: string
}

export type CreateElement = {
    Id: Id,
    Tag: string
}

export type AppendChild = {
    ParentId: Id,
    NewId: Id,
}

export type SetTextNodeValue = {
    Id: Id,
    Text: string
}

export type InsertBefore = {
    ParentId: Id,
    NewId: Id,
    BeforeId: Id
}

export type RemoveChild = {
    ParentId: Id,
    ChildId: Id
}

function isDOMCommandCore(value: any, type_dev: string, type_prod: number) {
    if(window["EXPRAZOR_DEV"]) {
        return value.Type == type_dev;
    }

    return value.Type == type_prod;
}
export function isSetStringAttribute(value: any) : value is SetStringAttribute {
    return isDOMCommandCore(value, "SetStringAttribute", 1);
}
export function isSetNumberAttribute(value: any): value is SetNumberAttribute {
    return isDOMCommandCore(value, "SetNumberAttribute", 2);
}
export function isSetBooleanAttribute(value: any): value is SetBooleanAttribute {
    return isDOMCommandCore(value, "SetBooleanAttribute", 3);
}
export function isRemoveAttribute(value: any): value is RemoveAttribute {
    return isDOMCommandCore(value, "RemoveAttribute", 4);
}
export function isSetVoidCallback(value:any): value is SetVoidCallback {
    return isDOMCommandCore(value, "SetVoidCallback", 5);
}
export function isRemoveCallback(value: any):value is RemoveCallback {
    return isDOMCommandCore(value, "RemoveCallback", 6);
}
export function isCreateTextNode(value:any):value is CreateTextNode {
    return isDOMCommandCore(value, "CreateTextNode", 7);
}
export function isCreateElement(value:any):value is CreateElement {
    return isDOMCommandCore(value, "CreateElement", 8);
}
export function isAppendChild(value:any):value is AppendChild {
    return isDOMCommandCore(value, "AppendChild", 9 );
}
export function isSetTextNodeValue(value:any): value is SetTextNodeValue {
    return isDOMCommandCore(value, "SetTextNodeValue", 10);
}
export function isInsertBefore(value:any):value is InsertBefore {
    return isDOMCommandCore(value, "InsertBefore", 11);
}
export function isRemoveChild(value:any):value is RemoveChild {
    return isDOMCommandCore(value, "RemoveChild", 12);
}