# BGinfo
Пока жалкий аналаог программы BGinfo из пакета Sysinternals для  Windows 10.

Умеет отображать имя хоста на:
1. экране блокировки (приветсвия/ выбора пользователя/ ввода пароля);
2. обоях пользователей.

Состоит из двух программ:

1. LockscreenBGinfo.exe - программа для экрана блокировки и по совместительсту установщик пакета.
2. DesktopBGinfo.exe - программа для обоев пользователя.

Действия LockscreenBGinfo.exe при запуске из сессии пользователя с правами Администратора:
1. Копирует себя (LockscreenBGinfo.exe), DesktopBGinfo.exe в папку \ProgramFiles\BGinfo\;
2. Копирует файлы логотипов LockScreenLogo.jpg и DeskTopLogo.jpg в папку \Windows\System32\oobe\info\backgrounds\
3. Создает задание в планировщике Windows на запуск себя во время загрузки ОС;
4. Создает в разделе реестра HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run параметр BGInfo REG_SZ \ProgramFiles\BGinfo\DesktopBGinfo.exe для запуска DesktopBGinfo.exe при входе каждого пользователя.
5. Создает в разделе реестра HKEY_LOCAL_MACHINE\SOFTWARE\ раздел BGInfo для хранения конфигурации программы.

Действия LockscreenBGinfo.exe при загрузки ОС:
1.Создает файл для отображения на экране блокировки. Имя файла имеет формат <hostname>-<ScreenWidth>x<ScreenHeight>.jpg
2.Изменяет в разделе реестра HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP параметры:
  LockScreenImagePath  
  LockScreenImageUrl
  LockScreenImageStatus

Действия DesktopBGinfo.exe:
1.Находит кэш текущих обоев в папке и %APPDATA%\Microsoft\Windows\Themes\  (\Users\<User name>\AppData\Roaming\Microsoft\Windows\Themes\ и добавляет имя хоста
