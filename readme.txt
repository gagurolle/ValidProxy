Используется .net core 3.1 . Работает только на linux, в частности, тестировалось на ubuntu без gui. Браузер Chrome запускается в режиме HEADLESS.
Также, для корректной работы браузера в Ubuntu, для запуска браузера был добавлен аттрибут --no-sandbox, в связи с чем, запуск на Windows стал
не корректным. Для того, чтобы работал в Windows, стоит заккоментировать добавление этого параметра в методе GetWebSiteElements().
TODO: сделать проставление данного флага автоматически в зависимости от выбранной ОС.
