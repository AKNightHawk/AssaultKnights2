   M S B A M L   `   `   `  ����J  FWPFAppFramework, Version=3.2.0.0, Culture=neutral, PublicKeyToken=nullJ6clr-namespace:WPFAppFramework;assembly=WPFAppFrameworkWPFAppFramework  5       WPFAppExample   WPFAppExample.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      RWPFAppFramework6clr-namespace:WPFAppFramework;assembly=WPFAppFramework     5      -   5          � Icon$  Logo.ico��5   	    � Title$ WPF Application Example��5      $	��592��6%   $	��807��62    � WindowStartupLocation$ CenterScreen=�6l    J� SnapsToDevicePixels .  .��5      � -   5         �Name$ 
LayoutRoot��5      $
��Auto��6   $
��Auto��6-    .   � RowDefinitions 5   
   �� 5	      $
��594* �6   �� 5
      $��* �6   .+�5   
   -   'WPFAppFramework.RenderTargetUserControl  -   5        $��renderTargetUserControl1��5   2   $��12,115,104,35q�6R      AllowUsingD3DImage . 6i   �� 5   
   -   5        $��slider1��5   A   $��Right=�6   $��
0,300,13,0q�6-   $��75��6P    �� TickPlacement$ BottomRight=�6    � Value$ 25Q�6�   	 � Maximum$		 100Q�6�   
 � LargeChange$
 50Q�6�    �� TickFrequency$ 10Q�6�    � SmallChange$ 1Q�6�   $��28��6�   $	��Top=�6�   �� 5   
   -   5        $��buttonClose��5   L   $��23��6   $��Right=�6   $��	0,12,13,0q�69   $	��Top=�6_   $��75��6w   .��6�   Exit6�   �� 5   
   -   5        $��label1��5   X   $��Right=�6   $	��Top=�6,   $��
0,268,13,0q�6D   $��75��6f   $��25��6q   .��6}   Volume6�   �� 5   
   -   5        $��buttonAddForm��5   M   $��23��6   $��Right=�6   $��
0,101,13,0q�69   $	��Top=�6b   $��75��6z   .��6�   
New Form6�   �� 5   
   -   5        $��buttonCreateBox��5   M   $��23��6   $��Right=�6   $��
0,178,13,0q�69   $	��Top=�6d   $��75��6|   .��6�   
Create box6�   �� 5   
   -	   5        $��buttonShowUI��5   M   $��23��6   $��Right=�6   $��
0,207,13,0q�69   $	��Top=�6a   $��75��6y   $��Center=�6�   $��Center=�6�   .��6�   	Show UI6�   �� 5   
   -
   5        $��label2��5   I   $��25��6   $
��Left=�6   $��12,0,0,3q�67   $��Bottom=�6W   $	��226��6r   .��6~   2014 NeoAxis Group Ltd.6�   �� 5   
   -   5        $��label3��5   J   $��25��6   $
��Left=�6   $��	12,12,0,0q�67   $	��Top=�6X   $��50��6p   .��6{   Map:6�   �� 5   
   -   5        $��comboBoxMaps��5   M   $��23��6   $
��Left=�6   $��	66,13,0,0q�6:   $	��Top=�6a   $	��247��6y   �� 5   
   -   5        $��buttonLoadMap��5   1   $��23��6   $��
319,13,0,0q�6   $	��Top=�6F   $
��Left=�6z   $��75��6�   .��6�   Load6�   �� 5   
   -   5        $��buttonDestroy��5   L   $��23��6   $
��Left=�6   $��
464,13,0,0q�68   $	��Top=�6a   $��75��6y   .��6�   	Destroy6�   �� 5   
   -   5        $��label4��5   J   $��25��6   $
��Left=�6   $��	12,46,0,0q�67   $	��Top=�6X   $	��134��6p   .��6|   Connect to server:6�   �� 5   
   -   5        $��textBoxServerAddress��5   M   $��23��6   $
��Left=�6   $��
121,48,0,0q�69   $	��Top=�6i   $	��137��6�   .��6�   	127.0.0.16�   �� 5   
   -   5        $��textBoxUserName��5   2   $��23��6   $��
266,48,0,0q�6   $	��Top=�6I   $
��Left=�6a   $��93��6|   .��6�   
TestUser6�   �� 5   
   -   5        $��buttonConnect��5   1   $��23��6   $��
367,48,0,0q�6   $	��Top=�6F   $
��Left=�6^   $��75��6y   .��6�   	Connect6�   �� 5   
   -   5        $��labelNetworkStatus��5   K   $��23��6   $
��Left=�6   $��
444,47,0,0q�67   $	��Top=�6e   $	��121��6}   $��0q�6�   .��6�   
{Status}6�   �� 5   
   -   5        $��checkBoxUseD3DImage��5   M   $��16��6   $
��Left=�6   $��	12,87,0,0q�6:   $	��Top=�6h   $	��151��6�    d� 	IsChecked$
 TrueY�6�   .��6�   Use D3DImage output6  5      5      