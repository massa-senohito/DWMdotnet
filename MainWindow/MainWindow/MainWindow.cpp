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

#include "conf.h"

int by = 0;
int ww = 0;
int bh = 0;
void
drawbar( void ) {
  dc.hdc = GetWindowDC( barhwnd );

  dc.h = bh;

  int x;
  unsigned int i , occ = 0 , urg = 0;
  unsigned long *col;
  Client *c;

  // occにクライアントのタグのフラグを立てていく 緊急なら緊急のクライアントのフラグを立てる
  for( c = clients; c; c = c->next ) {
    occ |= c->tags;
    if( c->isurgent )
      urg |= c->tags;
  }

  dc.x = 0;
  for( i = 0; i < LENGTH( tags ); i++ ) {
    dc.w = TEXTW( tags[i] );
    // 選ばれているなら選ばれている色で書く
    col = tagset[seltags] & 1 << i ? dc.sel : dc.norm;
    drawtext( tags[i] , col , urg & 1 << i );
    drawsquare( sel && sel->tags & 1 << i , occ & 1 << i , urg & 1 << i , col );
    dc.x += dc.w;
  }
  // レイアウトシンボルの最大設定をdcのwに入れ、ローカルxに dc.x+レイアウトシンボルの最大設定 を入れる
  if( blw > 0 ) {
    dc.w = blw;
    drawtext( lt[sellt]->symbol , dc.norm , false );
    x = dc.x + dc.w;
  }
  else
    x = dc.x;

  // グローバルww -stextのwidth
  dc.w = TEXTW( stext );
  dc.x = ww - dc.w;
  if( dc.x < x ) {
    dc.x = x;
    dc.w = ww - x;
  }
  drawtext( stext , dc.norm , false );

  if( ( dc.w = dc.x - x ) > bh ) {
    dc.x = x;
    if( sel ) {
      drawtext( getclienttitle( sel->hwnd ) , dc.sel , false );
      drawsquare( sel->isfixed , sel->isfloating , false , dc.sel );
    }
    else
      drawtext( NULL , dc.norm , false );
  }

  ReleaseDC( barhwnd , dc.hdc );
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
    drawbar( );
    EndPaint( hwnd , &ps );
    break;
  }
  case WM_LBUTTONDOWN:
  case WM_RBUTTONDOWN:
  case WM_MBUTTONDOWN:
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

	if (!RegisterClass(&winClass))
		die("Error registering window class");


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

	for(blw = i = 0; LENGTH(layouts) > 1 && i < LENGTH(layouts); i++) {
 		w = TEXTW(layouts[i].symbol);
		blw = MAX(blw, w);
	}

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