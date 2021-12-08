(() => {
  // src/ClientCommand.ts
  function isHandleCommandsProd(value) {
    return value.Type === 0;
  }
  function isHandleCommandsDev(value) {
    return value.Type === "HandleCommands";
  }
  var isHandleCommands = __DEV__ ? isHandleCommandsDev : isHandleCommandsProd;

  // src/DOMCommands.ts
  function isSetStringAttributeProd(value) {
    return value.Type === 1;
  }
  function isSetStringAttributeDev(value) {
    return value.Type === "SetStringAttribute";
  }
  var isSetStringAttribute = __DEV__ ? isSetStringAttributeDev : isSetStringAttributeProd;
  function isSetNumberAttributeProd(value) {
    return value.Type === 2;
  }
  function isSetNumberAttributeDev(value) {
    return value.Type === "SetNumberAttribute";
  }
  var isSetNumberAttribute = __DEV__ ? isSetNumberAttributeDev : isSetNumberAttributeProd;
  function isSetBooleanAttributeProd(value) {
    return value.Type === 3;
  }
  function isSetBooleanAttributeDev(value) {
    return value.Type === "SetBooleanAttribute";
  }
  var isSetBooleanAttribute = __DEV__ ? isSetBooleanAttributeDev : isSetBooleanAttributeProd;
  function isRemoveAttributeProd(value) {
    return value.Type === 4;
  }
  function isRemoveAttributeDev(value) {
    return value.Type === "RemoveAttribute";
  }
  var isRemoveAttribute = __DEV__ ? isRemoveAttributeDev : isRemoveAttributeProd;
  function isSetVoidCallbackProd(value) {
    return value.Type === 5;
  }
  function isSetVoidCallbackDev(value) {
    return value.Type === "SetVoidCallback";
  }
  var isSetVoidCallback = __DEV__ ? isSetVoidCallbackDev : isSetVoidCallbackProd;
  function isRemoveCallbackProd(value) {
    return value.Type === 6;
  }
  function isRemoveCallbackDev(value) {
    return value.Type === "RemoveCallback";
  }
  var isRemoveCallback = __DEV__ ? isRemoveCallbackDev : isRemoveCallbackProd;
  function isCreateTextNodeProd(value) {
    return value.Type === 7;
  }
  function isCreateTextNodeDev(value) {
    return value.Type === "CreateTextNode";
  }
  var isCreateTextNode = __DEV__ ? isCreateTextNodeDev : isCreateTextNodeProd;
  function isCreateElementProd(value) {
    return value.Type === 8;
  }
  function isCreateElementDev(value) {
    return value.Type === "CreateElement";
  }
  var isCreateElement = __DEV__ ? isCreateElementDev : isCreateElementProd;
  function isAppendChildProd(value) {
    return value.Type === 9;
  }
  function isAppendChildDev(value) {
    return value.Type === "AppendChild";
  }
  var isAppendChild = __DEV__ ? isAppendChildDev : isAppendChildProd;
  function isSetTextNodeValueProd(value) {
    return value.Type === 10;
  }
  function isSetTextNodeValueDev(value) {
    return value.Type === "SetTextNodeValue";
  }
  var isSetTextNodeValue = __DEV__ ? isSetTextNodeValueDev : isSetTextNodeValueProd;
  function isInsertBeforeProd(value) {
    return value.Type === 11;
  }
  function isInsertBeforeDev(value) {
    return value.Type === "InsertBefore";
  }
  var isInsertBefore = __DEV__ ? isInsertBeforeDev : isInsertBeforeProd;
  function isRemoveChildProd(value) {
    return value.Type === 12;
  }
  function isRemoveChildDev(value) {
    return value.Type === "RemoveChild";
  }
  var isRemoveChild = __DEV__ ? isRemoveChildDev : isRemoveChildProd;

  // src/index.ts
  var idToElement = /* @__PURE__ */ new Map();
  var elementToId = /* @__PURE__ */ new Map();
  var MOUNT_ID = -1;
  idToElement[MOUNT_ID] = document.querySelector("body");
  elementToId[idToElement[MOUNT_ID]] = MOUNT_ID;
  var location = window.location;
  var hubUri = `${location.protocol === "https:" ? "wss:" : "ws:"}//${location.host}${location.pathname}counter/123`;
  var socket = new WebSocket(hubUri);
  socket.addEventListener("open", (event) => {
    socket.send(JSON.stringify(["Hello"]));
  });
  socket.addEventListener("message", (event) => {
    if (isHandleCommands(event.data)) {
      event.data.Commands.forEach((cmd) => {
        if (isSetStringAttribute(cmd)) {
          idToElement.get(cmd.Id).setAttribute(cmd.Key, cmd.Value);
        } else if (isSetNumberAttribute(cmd)) {
          idToElement.get(cmd.Id).setAttribute(cmd.Key, cmd.Value.toString());
        } else if (isSetBooleanAttribute(cmd)) {
          if (cmd.Value) {
            idToElement.get(cmd.Id).setAttribute(cmd.Key, "");
          } else {
            idToElement.get(cmd.Id).removeAttribute(cmd.Key);
          }
        } else if (isRemoveAttribute(cmd)) {
          idToElement.get(cmd.Id).removeAttribute(cmd.Key);
        } else if (isSetVoidCallback(cmd)) {
          switch (cmd.Key) {
            default:
              var type = __DEV__ ? "invokeVoid" : 1;
              idToElement.get(cmd.Id)[cmd.Key] = () => socket.send(JSON.stringify([type, cmd.Id, cmd.Key]));
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
        } else if (isSetTextNodeValue(cmd)) {
          idToElement.get(cmd.Id).textContent = cmd.Text;
        } else if (isInsertBefore(cmd)) {
          idToElement.get(cmd.ParentId).insertBefore(idToElement.get(cmd.NewId), idToElement.get(cmd.BeforeId));
        } else if (isRemoveChild(cmd)) {
          const childToRemove = idToElement.get(cmd.ChildId);
          if (childToRemove instanceof HTMLElement) {
            for (const child of childToRemove.querySelectorAll("*")) {
              var id = elementToId.get(child);
              idToElement.delete(id);
              elementToId.delete(child);
            }
          }
          idToElement[cmd.ParentId].removeChild(childToRemove);
        }
      });
    }
  });
})();
//# sourceMappingURL=exprazor-client.js.map
