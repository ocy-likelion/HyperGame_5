var BridgePlugin = {
    // 단순 보이드 메서드 실행 용도
    ExecuteJavaScriptMethod: function (method) {
        var jsMethod = UTF8ToString(method);
        try {
            eval(jsMethod);
        } catch(error){
            console.error('JavaScript Error : ', error);
        }
    },
    // 값을 반환하는 용도
    ExecuteJavaScriptReturnString: function (methodPtr) {
        var jsMethod = Module.UTF8ToString(methodPtr);
        try {

            var result = eval(jsMethod);
            if (result === undefined || result === null) return "";
            return result.toString();
        } catch (error) {
            console.error('JavaScript Error:', error);
            return "error";
        }
    }
};

mergeInto(LibraryManager.library, BridgePlugin);