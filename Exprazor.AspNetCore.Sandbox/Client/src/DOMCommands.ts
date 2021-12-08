import { getIsDevelopment } from "./Utils";

declare var __DEV__ : any;

export type Id = number;

export type DOMCommand = 
    SetNumberAttribute  |
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

function isSetStringAttributeProd(value: any) : value is SetStringAttribute {
    return value.Type === 1;
}
function isSetStringAttributeDev(value:any) : value is SetStringAttribute {
    return value.Type === "SetStringAttribute"
}
export const isSetStringAttribute : (value:any) => value is SetStringAttribute = (__DEV__ ? isSetStringAttributeDev : isSetStringAttributeProd);

function isSetNumberAttributeProd(value: any) : value is SetNumberAttribute {
    return value.Type === 2;
}
function isSetNumberAttributeDev(value:any) : value is SetNumberAttribute {
    return value.Type === "SetNumberAttribute"
}
export const isSetNumberAttribute : (value:any) => value is SetNumberAttribute = (__DEV__ ? isSetNumberAttributeDev : isSetNumberAttributeProd);


function isSetBooleanAttributeProd(value: any) : value is SetBooleanAttribute {
    return value.Type === 3;
}
function isSetBooleanAttributeDev(value:any) : value is SetBooleanAttribute {
    return value.Type === "SetBooleanAttribute"
}
export const isSetBooleanAttribute : (value:any) => value is SetBooleanAttribute = (__DEV__ ? isSetBooleanAttributeDev : isSetBooleanAttributeProd);


function isRemoveAttributeProd(value: any) : value is RemoveAttribute {
    return value.Type === 4;
}
function isRemoveAttributeDev(value:any) : value is RemoveAttribute {
    return value.Type === "RemoveAttribute"
}
export const isRemoveAttribute : (value:any) => value is RemoveAttribute = (__DEV__ ? isRemoveAttributeDev : isRemoveAttributeProd);



function isSetVoidCallbackProd(value: any) : value is SetVoidCallback {
    return value.Type === 5;
}
function isSetVoidCallbackDev(value:any) : value is SetVoidCallback {
    return value.Type === "SetVoidCallback"
}
export const isSetVoidCallback : (value:any) => value is SetVoidCallback = (__DEV__ ? isSetVoidCallbackDev : isSetVoidCallbackProd);



function isRemoveCallbackProd(value: any) : value is RemoveCallback {
    return value.Type === 6;
}
function isRemoveCallbackDev(value:any) : value is RemoveCallback {
    return value.Type === "RemoveCallback"
}
export const isRemoveCallback : (value:any) => value is RemoveCallback = (__DEV__ ? isRemoveCallbackDev : isRemoveCallbackProd);



function isCreateTextNodeProd(value: any) : value is CreateTextNode {
    return value.Type === 7;
}
function isCreateTextNodeDev(value:any) : value is CreateTextNode {
    return value.Type === "CreateTextNode"
}
export const isCreateTextNode : (value:any) => value is CreateTextNode = (__DEV__ ? isCreateTextNodeDev : isCreateTextNodeProd);



function isCreateElementProd(value: any) : value is CreateElement {
    return value.Type === 8;
}
function isCreateElementDev(value:any) : value is CreateElement {
    return value.Type === "CreateElement"
}
export const isCreateElement : (value:any) => value is CreateElement = (__DEV__ ? isCreateElementDev : isCreateElementProd);


function isAppendChildProd(value: any) : value is AppendChild {
    return value.Type === 9;
}
function isAppendChildDev(value:any) : value is AppendChild {
    return value.Type === "AppendChild"
}
export const isAppendChild : (value:any) => value is AppendChild = (__DEV__ ? isAppendChildDev : isAppendChildProd);



function isSetTextNodeValueProd(value: any) : value is SetTextNodeValue {
    return value.Type === 10;
}
function isSetTextNodeValueDev(value:any) : value is SetTextNodeValue {
    return value.Type === "SetTextNodeValue"
}
export const isSetTextNodeValue : (value:any) => value is SetTextNodeValue = (__DEV__ ? isSetTextNodeValueDev : isSetTextNodeValueProd);


function isInsertBeforeProd(value: any) : value is InsertBefore {
    return value.Type === 11;
}
function isInsertBeforeDev(value:any) : value is InsertBefore {
    return value.Type === "InsertBefore"
}
export const isInsertBefore : (value:any) => value is InsertBefore = (__DEV__ ? isInsertBeforeDev : isInsertBeforeProd);


function isRemoveChildProd(value: any) : value is RemoveChild {
    return value.Type === 12;
}
function isRemoveChildDev(value:any) : value is RemoveChild {
    return value.Type === "RemoveChild"
}
export const isRemoveChild : (value:any) => value is RemoveChild = (__DEV__ ? isRemoveChildDev : isRemoveChildProd);



// export function isSetNumberAttribute(value : any) : value is SetNumberAttribute {
//     return isDOMCommandCore(value, "SetNumberAttribute", 2);
// }
// export function isSetBooleanAttribute(value : any) : value is SetBooleanAttribute {
//     return isDOMCommandCore(value, "SetBooleanAttribute", 3);
// }
// export function isRemoveAttribute(value: any) : value is RemoveAttribute {
//     return isDOMCommandCore(value, "RemoveAttribute", 4);
// }
// export function isSetVoidCallback(value:any): value is SetVoidCallback {
//     return isDOMCommandCore(value, "SetVoidCallback", 5);
// }
// export function isRemoveCallback(value: any):value is RemoveCallback {
//     return isDOMCommandCore(value, "RemoveCallback", 6);
// }
// export function isCreateTextNode(value:any):value is CreateTextNode {
//     return isDOMCommandCore(value, "CreateTextNode", 7);
// }
// export function isCreateElement(value:any):value is CreateElement {
//     return isDOMCommandCore(value, "CreateElement", 8);
// }
// export function isAppendChild(value:any):value is AppendChild {
//     return isDOMCommandCore(value, "AppendChild", 9 );
// }
// export function isSetTextNodeValue(value:any): value is SetTextNodeValue {
//     return isDOMCommandCore(value, "SetTextNodeValue", 10);
// }
// export function isInsertBefore(value:any):value is InsertBefore {
//     return isDOMCommandCore(value, "InsertBefore", 11);
// }
// export function isRemoveChild(value:any):value is RemoveChild {
//     return isDOMCommandCore(value, "RemoveChild", 12);
// }