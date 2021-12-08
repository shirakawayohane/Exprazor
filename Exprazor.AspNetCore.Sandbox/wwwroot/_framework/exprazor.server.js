(() => {
  // src/ClientCommand.ts
  function isHandleCommandsDev(value) {
    return value.Type === "HandleCommands";
  }
  var isHandleCommands = true ? isHandleCommandsDev : isHandleCommandsProd;

  // src/DOMCommands.ts
  function isSetStringAttributeDev(value) {
    return value.Type === "SetStringAttribute";
  }
  var isSetStringAttribute = true ? isSetStringAttributeDev : isSetStringAttributeProd;
  function isSetNumberAttributeDev(value) {
    return value.Type === "SetNumberAttribute";
  }
  var isSetNumberAttribute = true ? isSetNumberAttributeDev : isSetNumberAttributeProd;
  function isSetBooleanAttributeDev(value) {
    return value.Type === "SetBooleanAttribute";
  }
  var isSetBooleanAttribute = true ? isSetBooleanAttributeDev : isSetBooleanAttributeProd;
  function isRemoveAttributeDev(value) {
    return value.Type === "RemoveAttribute";
  }
  var isRemoveAttribute = true ? isRemoveAttributeDev : isRemoveAttributeProd;
  function isSetVoidCallbackDev(value) {
    return value.Type === "SetVoidCallback";
  }
  var isSetVoidCallback = true ? isSetVoidCallbackDev : isSetVoidCallbackProd;
  function isRemoveCallbackDev(value) {
    return value.Type === "RemoveCallback";
  }
  var isRemoveCallback = true ? isRemoveCallbackDev : isRemoveCallbackProd;
  function isCreateTextNodeDev(value) {
    return value.Type === "CreateTextNode";
  }
  var isCreateTextNode = true ? isCreateTextNodeDev : isCreateTextNodeProd;
  function isCreateElementDev(value) {
    return value.Type === "CreateElement";
  }
  var isCreateElement = true ? isCreateElementDev : isCreateElementProd;
  function isAppendChildDev(value) {
    return value.Type === "AppendChild";
  }
  var isAppendChild = true ? isAppendChildDev : isAppendChildProd;
  function isSetTextNodeValueDev(value) {
    return value.Type === "SetTextNodeValue";
  }
  var isSetTextNodeValue = true ? isSetTextNodeValueDev : isSetTextNodeValueProd;
  function isInsertBeforeDev(value) {
    return value.Type === "InsertBefore";
  }
  var isInsertBefore = true ? isInsertBeforeDev : isInsertBeforeProd;
  function isRemoveChildDev(value) {
    return value.Type === "RemoveChild";
  }
  var isRemoveChild = true ? isRemoveChildDev : isRemoveChildProd;

  // src/index.ts
  var idToElement = /* @__PURE__ */ new Map();
  var elementToId = /* @__PURE__ */ new Map();
  var MOUNT_ID = -1;
  idToElement.set(0, document.querySelector("#root"));
  console.log(idToElement);
  elementToId[idToElement[MOUNT_ID]] = MOUNT_ID;
  var location = window.location;
  var hubUri = `${location.protocol === "https:" ? "wss:" : "ws:"}//${location.host}${location.pathname}counter/123`;
  var socket = new WebSocket(hubUri);
  socket.addEventListener("open", (event) => {
    socket.send(JSON.stringify(["Hello"]));
  });
  socket.addEventListener("message", (event) => {
    console.log("eventdata : ", event);
    const data = JSON.parse(event.data);
    if (isHandleCommands(data)) {
      data.Commands.forEach((cmd) => {
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
              var type = true ? "InvokeVoid" : 1;
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
          console.log(idToElement);
          console.log(cmd.ParentId);
          console.log(idToElement.get(cmd.ParentId));
          console.log(idToElement.has(cmd.ParentId));
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
//# sourceMappingURL=exprazor.server.js.map
