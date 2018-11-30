# WCF_Chat

# Все указанные пути - относительно корневой папки склонированного репозитория.

1) Собирать при помощи Visual studio.
\WCF_Chat\WCF_Chat\bin\Debug - Исходники серверной части.
\WCF_Chat\ChatHost\bin\HostDebug - Скомпилированный runner сервера. Запуск ChatHost.exe только от имени администратора. В этой же директории будет создано файловое хранилище серверной части.
\WCF_Chat\ChatClient\bin\ClientDebug\ChatClient.exe - Клиентское приложение.
2) Unit tests запускать из проекта ChatUnitTests файл LoginValidatorTests.cs. Поскольку времени (не данного, а у нас) к сожалению не хватило, то Unit тестов не много. Только чтобы показать, что мы умеем.
3) Чат работает при помощи WCF.
	1. Запуск сервера - запустить ChatHost.exe от администратора
	2. Запустить ChatClient.exe
	3. Чтобы зарегистрировать пользователя нужно установить соответствующий чекбокс в окне логина и ввести логин и пароль.
	4. Чтобы добавить контакт нужно в главном окне нажать на кнопку AddNew и в появившееся поле вписать Username пользователя, которого Вы хотите добавить. После чего нажать на кнопку Add.
	5. Чтобы написать пользователю сообщение нужно выбрать его в списке контактов, ну а дальше понятно...
4) ООП: 
	\WCF_Chat\WCF_Chat\Entities\ServerUser.cs строка 5. 
	Унаследованный от ClientUser, ServerUser имеет поле, которое используется при работе сервера, но не требуется для работы IStorageHandler или клиентской части, поэтому пересылка и вся остальная работа ведётся через ClientUser.
5) Pattern: DI
	\WCF_Chat\WCF_Chat\ChatService.cs строка 17, 21.
	Отвязывание серверной логики от разновидности использованного хранилища.