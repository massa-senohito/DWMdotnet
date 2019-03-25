// F# の詳細については、http://fsharp.org を参照してください
// 詳細については、'F# チュートリアル' プロジェクトを参照してください。
namespace DWMDotnet
  module DWM =
    open Handles
    open Types
    open Handles
    open System.Diagnostics
    open System
    open System.Collections.Generic
    open System.Drawing

    type Native = ThreadWindowHandles
    type HWND       = IntPtr
    let  stext      = ""
    // スクリーンの広さ sx等に対応
    //let  screenGeom = new RECT()
    
    // by
    let barY      = 0
    let barHeight, barWidth = 0, 0
    
    // window area wxなど
    let windowGeom   = new RECT()
    let selectedTags, selLt = 0, 0
    
    let Rules    : Types.Rule list       = []
    //let ClientList  : Types.Client list ref = ref []
    let Selected : Types.Client list ref = ref []
    
    let Stack    : Types.Client list = []
    
    let textmargin = 5

    let textnw hdc text =
      textmargin + ThreadWindowHandles.GetTextExtentWidth(hdc,text)

    //let setSelected (client:Client) =
      

    let focus (client:Client) =
      
      ThreadWindowHandles.SetForegroundWindow(client.Hwnd)

    let setVisibility hwnd visiblity =
      let visibleFlag = if visiblity then SWP.SHOWWINDOW else SWP.HIDEWINDOW
      Native.SetWindowPos(hwnd , nativeint 0 , 0 , 0 , 0 , 0 , visibleFlag ||| SWP.NOACTIVATE ||| SWP.NOMOVE ||| SWP.NOSIZE ||| SWP.NOZORDER) |> ignore
      

    let setClientVisibility (client : Client ) visiblity =
      client.IsActive <- visiblity
      if(not visiblity) then client.Ignore <- true
      setVisibility client.Hwnd visiblity

    
    let Tags =
      [
      "1"
      "2"
      "3"
      "4"
      "5"
      "6"
      "7"
      "8"
      "9"
      ]
    let tagSet = [ 1 ; 1 ]
    //let isVisible (c:Types.Client) = c.Tags &&& tagSet.[selectedTags]
    let width  (c:Types.Client) = c.Rect.Width  + 2 * c.Bw
    let height (c:Types.Client) = c.Rect.Height + 2 * c.Bw
    let Tagmask = ( 1 <<< List.length Tags ) - 1
    
    let layout                        = [ nullLayouts ; nullLayouts ]
    let strFind (str:string) target   = 
      str.Contains(target)
    
    
    // list で全部変える方式にしたい
    //let showhide (c:Types.Client) =
    //  if( isVisible c > 0 ) then
    //    if ( Native.IsWindowVisible c.Hwnd > 0) then
    //      c.Ignore <- true
    //      c.Wasvisible <- true
    //      setVisibility c.Hwnd false
    //      ()
    //  else
    //    if c.Wasvisible then
    //      setVisibility c.Hwnd true
    
    //let applyRules (c:Types.Client) =
    //  for i in Rules do
    //    if( strFind (WinApiFs.getClientTitle c.Hwnd) i.Title 
    //        && strFind (WinApiFs.getClientClassname c.Hwnd) i.Class) then
    //          c.IsFloating <- i.Isfloating
    //          let tag = 
    //            if i.Tags &&& Tagmask > 0 then i.Tags &&& Tagmask else tagSet.[selectedTags]
    //          c.Tags <- c.Tags ||| tag
    //  if c.Tags = 0 then c.Tags <- tagSet.[selectedTags]
    
    //void
    //arrange(void) {
    //	showhide(stack);
    //	focus(NULL);
    //	if(lt[sellt]->arrange)
    //		lt[sellt]->arrange();
    //	restack();
    //}
    // 上から行ってるぶんは arrangeまで
    // すごくCっぽく書かれているので resizeなどから手を付ける
    //let arrange () =
    //  for i in Stack do
    //    showhide(i)
    
    //let attach c = //(ptr:IntPtr) =
      //let c =
      //  {
      //    Hwnd = ptr
      //    Parent = null
      //    Root = null
      //    ThreadId 

      //  }
    //  ClientList := c::ClientList.contents
    
    //let containClient ptr =
    //  ClientList.Value |> List.exists (fun c->c.Hwnd = ptr)

    let buttonPress button point =
      let mutable x = 0
      x 

    //let detach (client:Client) =
    //  let dropList = ClientList.Value |> List.filter (fun c->c.Hwnd <> client.Hwnd)
    //  ClientList := dropList
    
    let onHandle (handle) =
      true
    
    let resize (c:Types.Client) x y w h (scr:Rectangle) =
      let mutable copyX = x
      let mutable copyY = y
      let mutable copyW = w
      let mutable copyH = h
      if w <= 0 && h <= 0 then
        setClientVisibility c false
      else
        if x > scr.Right + scr.Width then
          copyX <- scr.Width - width c
        if y > scr.Top + scr.Height then
          copyY <- scr.Height - height c
        if x + w + c.Bw * 2 < scr.Left then
          copyX <- scr.Left
        if y + h + c.Bw * 2 < scr.Top then
          copyY <- scr.Top

        if h < barHeight then
          copyH <- barHeight
          // ?
        if w < barHeight then
          copyW <- barHeight
        let crect = c.Rect
        if crect.X <> copyX || crect.Y <> copyY || crect.Width <> copyW || crect.Height <> copyH then
          c.Rect <- new Rectangle(copyX , copyY , copyW , copyH )
          ThreadWindowHandles.SetWindowPos( c.Hwnd , nativeint(0) ,
            copyX , copyY , copyW , copyH , SWP.NOACTIVATE ) |> ignore


        
    
    //[<EntryPoint>]
    let main argv = 
        printfn "%A" argv
        let hs = new TopLevelWindowHandles()
        let hList = [for i in hs.handles -> i]
        for i in hList do
          let builder = ThreadWindowHandles.GetWindowText(i)
          if(builder <> null) then
            let name = builder.ToString()
            Debug.WriteLine(name)
            let flag = SWP.SHOWWINDOW ||| SWP.DRAWFRAME
            if name.IndexOf("diskinfo3",StringComparison.CurrentCultureIgnoreCase) <> 0 then
              ThreadWindowHandles.SetWindowPos(i,new nativeint(-1),600,90,500,500, flag) |> ignore
              ()
    
        0 // 整数の終了コードを返します
