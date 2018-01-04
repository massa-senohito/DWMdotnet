module Types
open System
open System.Runtime.InteropServices;

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

//typedef struct {
//	unsigned int click;
//	unsigned int button;
//	unsigned int key;
//	void (*func)(const Arg *arg);
//	const Arg arg;
//} Button;

type Client =
  {
    Hwnd:IntPtr
    Parent:IntPtr
    Root:IntPtr
    ThreadId:int
    Rect:System.Drawing.Rectangle
    Bw:int// XXX: useless?
    Tags:int
    IsMinimized:bool
    IsFloating:bool
    IsActive:bool
    Ignore:bool
    Border:bool
    Wasvisible:bool
    IsFixed:bool;    Isurgent:bool;    // XXX: useless?
    Next:Client
    Snext:Client
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