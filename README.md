# Кластеризация случаев мединского обслуживания

В данной работе рассматривается решается задача кластеризации и создания шаблонов мединского обслуживания. Наряд-заказы предсатвлены в виде кортежей категориальных данных.

Для решения задачи используется модифицированная версия алгорима CLOPE. Шаблоны выделяются из кластеров путем отсечения услуг, которые досточно редко в рамках кластера.

## Используемые средства
Проект создан на платформе microsoft dotnet framework 4.7.2 с помощью языка C#. Работа с базами данных осуществялется при помощи пакет Nuget MySql.Data.

Диаграмма классов:
![ClassDiagr](https://user-images.githubusercontent.com/57687004/196789680-c7ae200a-e8bb-489d-89bc-821fd062617a.png)
