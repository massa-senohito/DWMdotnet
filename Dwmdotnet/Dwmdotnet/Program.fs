// F# の詳細については、http://fsharp.org を参照してください
// 詳細については、'F# チュートリアル' プロジェクトを参照してください。
open Handles
open Types
open Handles
open System.Diagnostics
open System

type HWND       = IntPtr
let  stext      = ""
let  screenGeom = new NativeMethods.RECT()

let barY      = 0
let barHeight, barWidth = 0, 0

let windowGeom   = new NativeMethods.RECT()
let selectedTags, selLt = 0, 0

let Rules    : Types.Rule list       = []
let Clients  : Types.Client list ref = ref []
let Selected : Types.Client list ref = ref []

let Stack  :Types.Client list ref = ref []
let layout                        = [nullLayouts;nullLayouts]
let strFind (str:string) target   = 
  str.Contains(target)

//let applyRules (c:Types.Client) =
//  for i in Rules do
//    if( strFind (WinApi.getClientTitle c.Hwnd) i.Title 
//        && strFind (WinApi.getClientClassname c.Hwnd) i.Class) then
//          c
//    else
//      c

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

//let buttonPress button 

let onHandle (handle) =
  true

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    let hs = new NativeMethods.TopLevelWindowHandles()
    let hList = [for i in hs.handles -> i]
    for i in hList do
      let builder = NativeMethods.ThreadWindowHandles.GetWindowText(i)
      if(builder <> null) then
        let name = builder.ToString()
        Debug.WriteLine(name)
        let flag = NativeMethods.ThreadWindowHandles.SWP.SWP_SHOWWINDOW ||| NativeMethods.ThreadWindowHandles.SWP.SWP_DRAWFRAME
        if name.IndexOf("diskinfo3",StringComparison.CurrentCultureIgnoreCase) <> 0 then
          NativeMethods.ThreadWindowHandles.SetWindowPos(i,new nativeint(-1),600,90,500,500, flag) |> ignore
          ()

    0 // 整数の終了コードを返します
