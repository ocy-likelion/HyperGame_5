var BridgePlugin = {
    ExecuteJavaScriptMethod: function (method) {
        var jsMethod = UTF8ToString(method);
        try {
            eval(jsMethod);
        } catch(error){
            console.error('JavaScript Error : ', error);
        }
    }
};

mergeInto(LibraryManager.library, BridgePlugin);