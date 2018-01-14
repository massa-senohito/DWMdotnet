// MainWindow.cpp : メイン プロジェクト ファイルです。

#include "stdafx.h"


#define WIN32_LEAN_AND_MEAN
#define WINDOW_CLASS_NAME TEXT("WisdomSoft.Sample.Window")

/* variables */
static HWND dwmhwnd, barhwnd;

int origMain( HINSTANCE hInstance )
{
	HWND hWnd;
	WNDCLASS wc;

	/*ウィンドウクラスの登録*/
	wc.style = CS_HREDRAW | CS_VREDRAW;
	wc.lpfnWndProc = DefWindowProc;
	wc.cbClsExtra = 0;
	wc.cbWndExtra = 0;
	wc.hInstance = hInstance;
	wc.hIcon =  NULL;
	wc.hCursor = NULL;
	wc.hbrBackground = (HBRUSH)COLOR_BACKGROUND + 1;
	wc.lpszMenuName = NULL;
	wc.lpszClassName = WINDOW_CLASS_NAME ;

	if (!RegisterClass(&wc))
	{
		MessageBox(NULL, TEXT("ウィンドウクラスの作成に失敗しました"), NULL , MB_OK);
		return 0;
	}

	/*登録したクラスのウィンドウを生成*/
	hWnd = CreateWindow(
		WINDOW_CLASS_NAME , TEXT("Window Title"),
		WS_OVERLAPPEDWINDOW | WS_VISIBLE,
		100, 100, 400, 300,
		NULL, NULL, hInstance ,NULL
	);

	if (hWnd)
	{
		MessageBox(hWnd, TEXT("ウィンドウが生成されました"), TEXT("情報"), MB_OK);
	}
	else
	{
		MessageBox(NULL, TEXT("ウィンドウの生成に失敗しました"), NULL, MB_OK | MB_ICONERROR);
	}

	return 0;
}

/* enums */
enum { CurNormal , CurResize , CurMove , CurLast };		/* cursor */
enum { ColBorder , ColFG , ColBG , ColLast };			/* color */
enum { ClkTagBar , ClkLtSymbol , ClkStatusText , ClkWinTitle };	/* clicks */

/* function declarations */
//static void applyrules( Client *c );
//static void arrange( void );
//static void attach( Client *c );
//static void attachstack( Client *c );
//static void cleanup( );
//static void clearurgent( Client *c );
//static void detach( Client *c );
//static void detachstack( Client *c );
//static void die( const char *errstr , ... );
static void drawbar( void );
static void drawsquare( bool filled , bool empty , bool invert , unsigned long col[ColLast] );
static void drawtext( const char *text , unsigned long col[ColLast] , bool invert );
//void drawborder( Client *c , COLORREF color );
//void eprint( const char *errstr , ... );
//static void focus( Client *c );
//static void focusstack( const Arg *arg );
//static Client *getclient( HWND hwnd );
//LPSTR getclientclassname( HWND hwnd );
//LPSTR getclienttitle( HWND hwnd );
//HWND getroot( HWND hwnd );
//static void grabkeys( HWND hwnd );
//static void killclient( const Arg *arg );
//static Client *manage( HWND hwnd );
//static void monocle( void );
//static Client *nextchild( Client *p , Client *c );
//static Client *nexttiled( Client *c );
//static void quit( const Arg *arg );
//static void resize( Client *c , int x , int y , int w , int h );
//static void restack( void );
//static BOOL CALLBACK scan( HWND hwnd , LPARAM lParam );
//static void setborder( Client *c , bool border );
//static void setvisibility( HWND hwnd , bool visibility );
//static void setlayout( const Arg *arg );
//static void setmfact( const Arg *arg );
//static void setup( HINSTANCE hInstance );
//static void setupbar( HINSTANCE hInstance );
//static void showclientclassname( const Arg *arg );
//static void showhide( Client *c );
//static void spawn( const Arg *arg );
//static void tag( const Arg *arg );
static int textnw( const char *text , unsigned int len );
//static void tile( void );
//static void togglebar( const Arg *arg );
//static void toggleborder( const Arg *arg );
//static void toggleexplorer( const Arg *arg );
//static void togglefloating( const Arg *arg );
//static void toggletag( const Arg *arg );
//static void toggleview( const Arg *arg );
//static void unmanage( Client *c );
static void updatebar( void );
//static void updategeom( void );
//static void view( const Arg *arg );
//static void zoom( const Arg *arg );
#include "conf.h"
/* macros */
#define ISVISIBLE(x)            ((x)->tags & tagset[seltags])
#define ISFOCUSABLE(x)			(!(x)->isminimized && ISVISIBLE(x) && IsWindowVisible((x)->hwnd))
#define LENGTH(x)               (sizeof x / sizeof x[0])
#define MAX(a, b)               ((a) > (b) ? (a) : (b))
#define MIN(a, b)               ((a) < (b) ? (a) : (b))
#define MAXTAGLEN               16
#define WIDTH(x)                ((x)->w + 2 * (x)->bw)
#define HEIGHT(x)               ((x)->h + 2 * (x)->bw)
#define TAGMASK                 ((int)((1LL << LENGTH(tags)) - 1))
#define TEXTW(x)                (textnw(x, strlen(x)))

static char stext[256];

void
updatestatus(void)
{
  // setup時や　rootwindowの名前が変わると呼ばれる stextが無効ならstextにdwm-versionがコピー
	if (!gettextprop(root, XA_WM_NAME, stext, sizeof(stext)))
		strcpy(stext, "dwm-"VERSION);
	drawbar(selmon);
}

typedef struct {
  int x , y , w , h;
  unsigned long norm[ColLast];
  unsigned long sel[ColLast];
  HDC hdc;
} DC; /* draw context */

DC dc;

typedef union {
  int i;
  unsigned int ui;
  float f;
  void *v;
} Arg;

int by = 0;
int ww = 0;
int bh = 0;

static int blw;    /* bar geometry y, height and layout symbol width */

void
buttonpress( unsigned int button , POINTS *point ) {
  unsigned int i , x , click;
  Arg arg = { 0 };

  /* XXX: hack */
  dc.hdc = GetWindowDC( barhwnd );

  i = x = 0;

  do { x += TEXTW( tags[i] ); } while( point->x >= x && ++i < LENGTH( tags ) );
  if( i < LENGTH( tags ) ) {
    click = ClkTagBar;
    arg.ui = 1 << i;
  }
  else if( point->x < x + blw )
    click = ClkLtSymbol;
  else if( point->x > wx + ww - TEXTW( stext ) )
    click = ClkStatusText;
  else
    click = ClkWinTitle;

  if( GetKeyState( VK_SHIFT ) < 0 )
    return;

  // ルールにマッチするポインタの関数を呼び出す setlayout　など
  //for( i = 0; i < LENGTH( buttons ); i++ ) {
  //  if( click == buttons[i].click && buttons[i].func && buttons[i].button == button
  //    && ( !buttons[i].key || GetKeyState( buttons[i].key ) < 0 ) ) {
  //    buttons[i].func( click == ClkTagBar && buttons[i].arg.i == 0 ? &arg : &buttons[i].arg );
  //    break;
  //  }
  //}
}
void
drawbar( void ) {
  dc.hdc = GetWindowDC( barhwnd );

  //dc.h = bh;

  //int x;
  //unsigned int i , occ = 0 , urg = 0;
  //unsigned long *col;
  //Client *c;

  //// occにクライアントのタグのフラグを立てていく 緊急なら緊急のクライアントのフラグを立てる
  //for( c = clients; c; c = c->next ) {
  //  occ |= c->tags;
  //  if( c->isurgent )
  //    urg |= c->tags;
  //}

  //dc.x = 0;
  //for( i = 0; i < LENGTH( tags ); i++ ) {
  //  dc.w = TEXTW( tags[i] );
  //  // 選ばれているなら選ばれている色で書く
  //  col = tagset[seltags] & 1 << i ? dc.sel : dc.norm;
  //  drawtext( tags[i] , col , urg & 1 << i );
  //  drawsquare( sel && sel->tags & 1 << i , occ & 1 << i , urg & 1 << i , col );
  //  dc.x += dc.w;
  //}
  //// レイアウトシンボルの最大設定をdcのwに入れ、ローカルxに dc.x+レイアウトシンボルの最大設定 を入れる
  //if( blw > 0 ) {
  //  dc.w = blw;
  //  drawtext( lt[sellt]->symbol , dc.norm , false );
  //  x = dc.x + dc.w;
  //}
  //else
  //  x = dc.x;

  //// グローバルww -stextのwidth
  //dc.w = TEXTW( stext );
  //dc.x = ww - dc.w;
  //if( dc.x < x ) {
  //  dc.x = x;
  //  dc.w = ww - x;
  //}
  //drawtext( stext , dc.norm , false );

  //if( ( dc.w = dc.x - x ) > bh ) {
  //  dc.x = x;
  //  if( sel ) {
  //    drawtext( getclienttitle( sel->hwnd ) , dc.sel , false );
  //    drawsquare( sel->isfixed , sel->isfloating , false , dc.sel );
  //  }
  //  else
  //    drawtext( NULL , dc.norm , false );
  //}

  ReleaseDC( barhwnd , dc.hdc );
}
void
drawsquare( bool filled , bool empty , bool invert , COLORREF col[ColLast] ) {
  static int size = 5;
  RECT r;
  r.left = dc.x + 1;
  r.top = dc.y + 1 ;
  r.right = dc.x + size ;
  r.bottom = dc.y + size;

  HBRUSH brush = CreateSolidBrush( col[invert ? ColBG : ColFG] );
  SelectObject( dc.hdc , brush );

  if( filled ) {
    FillRect( dc.hdc , &r , brush );
  }
  else if( empty ) {
    FillRect( dc.hdc , &r , brush );
  }
  DeleteObject( brush );
}

void
drawtext( const char *text , COLORREF col[ColLast] , bool invert ) {
  RECT r;
  r.left = dc.x;
  r.top = dc.y;
  r.right = dc.x + dc.w;
  r.bottom = dc.y + dc.h;

  HPEN pen = CreatePen( PS_SOLID , borderpx , selbordercolor );
  HBRUSH brush = CreateSolidBrush( col[invert ? ColFG : ColBG] );
  SelectObject( dc.hdc , pen );
  SelectObject( dc.hdc , brush );
  FillRect( dc.hdc , &r , brush );

  DeleteObject( brush );
  DeleteObject( pen );

  SetBkMode( dc.hdc , TRANSPARENT );
  SetTextColor( dc.hdc , col[invert ? ColBG : ColFG] );

  HFONT font = ( HFONT )GetStockObject( SYSTEM_FONT );
  SelectObject( dc.hdc , font );

  DrawText( dc.hdc , text , -1 , &r , DT_CENTER | DT_VCENTER | DT_SINGLELINE );
}



LRESULT CALLBACK barhandler( HWND hwnd , UINT msg , WPARAM wParam , LPARAM lParam )
{
  switch( msg ) {
  case WM_CREATE:
    updatebar( );
    break;
  case WM_PAINT: {
    PAINTSTRUCT ps;
    BeginPaint( hwnd , &ps );
    // 中身をF#で保管する必要あり
    drawbar( );
    EndPaint( hwnd , &ps );
    break;
  }
  case WM_LBUTTONDOWN:
  case WM_RBUTTONDOWN:
  case WM_MBUTTONDOWN:
    // 中身をF#で保管する必要あり
    buttonpress( msg , &MAKEPOINTS( lParam ) );
    break;
  default:
    return DefWindowProc( hwnd , msg , wParam , lParam );
  }

  return 0;
}
void
setupbar(HINSTANCE hInstance) {

	unsigned int i, w = 0;

	WNDCLASS winClass;
	memset(&winClass, 0, sizeof winClass);

  LPCSTR title =
    //TEXT(
    "dwm-bar"//)
    ;

	winClass.style = 0;
	winClass.lpfnWndProc = barhandler;
	winClass.cbClsExtra = 0;
	winClass.cbWndExtra = 0;
	winClass.hInstance = hInstance;
	winClass.hIcon = NULL;
	winClass.hCursor = LoadCursor(NULL, IDC_ARROW);
	winClass.hbrBackground = NULL;
	winClass.lpszMenuName = NULL;
	winClass.lpszClassName = title;

	//if (!RegisterClass(&winClass))
	//	die("Error registering window class");


	barhwnd = CreateWindowEx(
		WS_EX_TOOLWINDOW,
		title,
		NULL, /* window title */
		WS_POPUP | WS_CLIPCHILDREN | WS_CLIPSIBLINGS,
		0, 0, 0, 0,
		NULL, /* parent window */
		NULL, /* menu */
		hInstance,
		NULL
	);

	/* calculate width of the largest layout symbol */
	dc.hdc = GetWindowDC(barhwnd);
	HFONT font = (HFONT)GetStockObject(SYSTEM_FONT);
	SelectObject(dc.hdc, font);

	//for(blw = i = 0; LENGTH(layouts) > 1 && i < LENGTH(layouts); i++) {
 //		w = TEXTW(layouts[i].symbol);
	//	blw = MAX(blw, w);
	//}

	ReleaseDC(barhwnd, dc.hdc);

	PostMessage(barhwnd, WM_PAINT, 0, 0);
	updatebar();
}
void
updatebar( void ) {
  SetWindowPos( barhwnd , showbar ? HWND_TOPMOST : HWND_NOTOPMOST , 0 , by , ww , bh , ( showbar ? SWP_SHOWWINDOW : SWP_HIDEWINDOW ) | SWP_NOACTIVATE | SWP_NOSENDCHANGING );
}

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow)
{
}