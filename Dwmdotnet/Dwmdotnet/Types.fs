module Types
open System
open System.Runtime.InteropServices;
open WinApi.User32
open Handles
open NetCoreEx.Geometry
open System.Text

//typedef struct {
//	int x, y, w, h;
//	unsigned long norm[ColLast];
//	unsigned long sel[ColLast];
//	HDC hdc;
//} DC;

type Arg =
  |I of int
  |UI of int
  |F of float32
  |V of obj

let getWinText hwnd =
  let len = User32Methods.GetWindowTextLength(hwnd)
  let builder = new StringBuilder(len + 1)
  User32Methods.GetWindowText( hwnd , builder , builder.Capacity )
  builder.ToString()
  

type Client =
  {
    Hwnd:IntPtr
    Parent:IntPtr
    Root:IntPtr
    ThreadId:int
    mutable Rect:System.Drawing.Rectangle
    Bw:int// XXX: useless?
    //mutable Tags:int
    //IsMinimized:bool
    //mutable IsFloating:bool
    mutable IsActive:bool
    mutable Ignore:bool
    //Border:bool
    //mutable Wasvisible:bool
    //IsFixed:bool;
    //Isurgent:bool;    // XXX: useless?
    //Next:Client
    //mutable Snext:Client
    Style : WindowStyles
    mutable Title:string
  }

  member this.HasSizeUpdate() =
    let rect = ref <|new Rectangle()
    let ok = User32Methods.GetWindowRect(this.Hwnd,rect)
    let rect = !rect
    let rect = new System.Drawing.Rectangle( rect.Left , rect.Top , rect.Width , rect.Height )
    let hasUpdate = this.Rect <> rect
    if hasUpdate then this.Rect<- rect
    hasUpdate

  member t.HasTitleUpdate() =
    let title = getWinText t.Hwnd
    let needUpdate = title <> t.Title
    if needUpdate then
      t.Title <- title
    needUpdate

  member t.CenterY =
    (t.Rect.Top + t.Rect.Height) / 2

  member private t.UpdateByRect() =
    User32Methods.SetWindowPos(t.Hwnd , nativeint(0) , t.Rect.X , t.Rect.Y , t.Rect.Width , t.Rect.Height , WindowPositionFlags.SWP_NOACTIVATE)
  
  member t.SetYH y h =
    t.Rect <- new System.Drawing.Rectangle(t.Rect.X , y , t.Rect.Width , h)
    t.UpdateByRect()

  member t.SetXW x w =
    t.Rect <- new System.Drawing.Rectangle(x , t.Rect.Top, w , t.Rect.Height)
    t.UpdateByRect()

let createClient hwnd parent threadId style title =
  {
    Hwnd = hwnd
    Parent = parent
    Root = IntPtr.Zero
    ThreadId = threadId
    Rect = new System.Drawing.Rectangle()
    Bw = 0
    IsActive = true
    Ignore = false
    Style = style
    Title = title
  }

type Key =
  {
    Mod:int
    Key:int
    Func:Arg->unit
    Arg:Arg
  }

//typedef struct {
//	const char *symbol;
//	void (*arrange)(void);
//} Layout;

type Layout =
  {
    Symbol:string 
    Arrange : unit->unit
  }
let nullLayouts = {Symbol = "" ; Arrange = fun ()->() }

type Rule =
  {
    Class:string;
    Title:string
    Tags:int;
    Isfloating:bool
  }

///* XXX: should be in a system header, no? */
//typedef struct {
//    HWND    hwnd;
//    RECT    rc;
//} SHELLHOOKINFO, *LPSHELLHOOKINFO;