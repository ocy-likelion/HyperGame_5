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
    ExecuteJavaScriptReturn: function(method) {
        var jsMethod = UTF8ToString(method);
        try {
            var result = eval(jsMethod); // JS 코드 실행
            var str = result ? result.toString() : "";
            var lengthBytes = lengthBytesUTF8(str) + 1;
            var buffer = _malloc(lengthBytes);
            stringToUTF8(str, buffer, lengthBytes);
            return buffer;
            //return allocateUTF8(result ? result.toString() : ""); // 문자열로 반환
        } catch (error) {
            console.error('JavaScript Error : ', error);
            var str = "error";
            var lengthBytes = lengthBytesUTF8(str) + 1;
            var buffer = _malloc(lengthBytes);
            stringToUTF8(str, buffer, lengthBytes);
            return buffer;
            //return allocateUTF8("error");
        }
    }
};

mergeInto(LibraryManager.library, BridgePlugin);