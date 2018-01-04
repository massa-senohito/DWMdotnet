// F# の詳細については、http://fsharp.org を参照してください
// 詳細については、'F# チュートリアル' プロジェクトを参照してください。
let Rules:Types.Rule list = []
let Clients:Types.Client list ref=ref []
let Stack  :Types.Client list ref=ref []

let strFind (str:string) target =
  str.Contains(target)

let applyRules (c:Types.Client) =
  for i in Rules do
    if( strFind (WinApi.getClientTitle c.Hwnd) i.Title 
        && strFind (WinApi.getClientClassname c.Hwnd) i.Class) then
          c
    else
      c

//void
//arrange(void) {
//	showhide(stack);
//	focus(NULL);
//	if(lt[sellt]->arrange)
//		lt[sellt]->arrange();
//	restack();
//}

let attach (c:Types.Client) =
  Clients := c::Clients.contents

let attachStack (c:Types.Client) =
  Stack:= c::Stack.contents

let buttonPress button 

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    0 // 整数の終了コードを返します
