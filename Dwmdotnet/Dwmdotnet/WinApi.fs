module WinApi
//let getClientTitle hWnd =
//  (Handles.ThreadWindowHandles.GetWindowText hWnd).ToString()
//let getClientClassname hWnd =
//  (Handles.ThreadWindowHandles.GetClassText hWnd).ToString()

let customWnd name proc =
  new CustomWindow(name , proc)
