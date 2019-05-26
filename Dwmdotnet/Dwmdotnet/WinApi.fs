module WinApiFs
let getClientTitle hWnd =
  (Handles.ThreadWindowHandles.GetWindowText hWnd).ToString()
let getClientClassname hWnd =
  (Handles.ThreadWindowHandles.GetClassText hWnd).ToString()


