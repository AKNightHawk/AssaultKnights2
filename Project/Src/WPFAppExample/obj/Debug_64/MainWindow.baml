   M S B A M L   `   `   `  ���� J  FWPFAppFramework, Version=3.2.0.0, Culture=neutral, PublicKeyToken=nullJ6clr-namespace:WPFAppFramework;assembly=WPFAppFrameworkWPFAppFramework   WPFAppExample   WPFAppExample.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     RWPFAppFramework6clr-namespace:WPFAppFramework;assembly=WPFAppFramework     -     � Icon$  Logo.ico�� � Title$ WPF Application Example��$	��592��$	��807�� � WindowStartupLocation$ CenterScreen=� J� SnapsToDevicePixels .  .��� -    �Name$ 
LayoutRoot��$
��Auto��$
��Auto�� .   � RowDefinitions �� $
��594* ��� $��* �.+�-   'WPFAppFramework.RenderTargetUserControl  -   $��renderTargetUserControl1��$��12,115,104,35q�   AllowUsingD3DImage . �� -   $��slider1��$��Right=�$��
0,300,13,0q�$��75�� �� TickPlacement$ BottomRight=� � Value$ 25Q�	 � Maximum$		 100Q�
 � LargeChange$
 50Q� �� TickFrequency$ 10Q� � SmallChange$ 1Q�$��28��$	��Top=��� -   $��buttonClose��$��23��$��Right=�$��	0,12,13,0q�$	��Top=�$��75��.��Exit�� -   $��label1��$��Right=�$	��Top=�$��
0,268,13,0q�$��75��$��25��.��Volume�� -   $��buttonAddForm��$��23��$��Right=�$��
0,101,13,0q�$	��Top=�$��75��.��
New Form�� -   $��buttonCreateBox��$��23��$��Right=�$��
0,178,13,0q�$	��Top=�$��75��.��
Create box�� -	   $��buttonShowUI��$��23��$��Right=�$��
0,207,13,0q�$	��Top=�$��75��$��Center=�$��Center=�.��	Show UI�� -
   $��label2��$��25��$
��Left=�$��12,0,0,3q�$��Bottom=�$	��226��.��2014 NeoAxis Group Ltd.�� -   $��label3��$��25��$
��Left=�$��	12,12,0,0q�$	��Top=�$��50��.��Map:�� -   $��comboBoxMaps��$��23��$
��Left=�$��	66,13,0,0q�$	��Top=�$	��247���� -   $��buttonLoadMap��$��23��$��
319,13,0,0q�$	��Top=�$
��Left=�$��75��.��Load�� -   $��buttonDestroy��$��23��$
��Left=�$��
464,13,0,0q�$	��Top=�$��75��.��	Destroy�� -   $��label4��$��25��$
��Left=�$��	12,46,0,0q�$	��Top=�$	��134��.��Connect to server:�� -   $��textBoxServerAddress��$��23��$
��Left=�$��
121,48,0,0q�$	��Top=�$	��137��.��	127.0.0.1�� -   $��textBoxUserName��$��23��$��
266,48,0,0q�$	��Top=�$
��Left=�$��93��.��
TestUser�� -   $��buttonConnect��$��23��$��
367,48,0,0q�$	��Top=�$
��Left=�$��75��.��	Connect�� -   $��labelNetworkStatus��$��23��$
��Left=�$��
444,47,0,0q�$	��Top=�$	��121��$��0q�.��
{Status}�� -   $��checkBoxUseD3DImage��$��16��$
��Left=�$��	12,87,0,0q�$	��Top=�$	��151�� d� 	IsChecked$
 TrueY�.��Use D3DImage output