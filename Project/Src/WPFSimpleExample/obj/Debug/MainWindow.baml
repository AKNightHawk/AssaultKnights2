   M S B A M L   `   `   `  ����J  FWPFAppFramework, Version=3.2.0.0, Culture=neutral, PublicKeyToken=nullJ6clr-namespace:WPFAppFramework;assembly=WPFAppFrameworkWPFAppFramework  5       WPFSimpleExample!   WPFSimpleExample.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      RWPFAppFramework6clr-namespace:WPFAppFramework;assembly=WPFAppFramework     5      -   5          � Icon$  Logo.ico��5   	    � Title$$ WPF Simple Application Example��5      $	��592��6,   $	��807��69    � WindowStartupLocation$ CenterScreen=�6s    J� SnapsToDevicePixels .  .��5      � -   5         �Name$ 
LayoutRoot��5      $
��Auto��6   $
��Auto��6-    .   � RowDefinitions 5   
   �� 5	      $
��594* �6   �� 5
      $��* �6   .+�5   
   -   'WPFAppFramework.RenderTargetUserControl  -   5        $��renderTargetUserControl1��5   2   $��12,51,12,35q�6R      AllowUsingD3DImage . 6g   �� 5   
   -   5        $��buttonClose��5   L   $��23��6   $��Right=�6   $��	0,12,13,0q�69   $	��Top=�6_   $��75��6w   .��6�   Exit6�   �� 5   
   -   5        $��label2��5   I   $��25��6   $
��Left=�6   $��12,0,0,3q�67   $��Bottom=�6W   $	��226��6r   .��6~   2014 NeoAxis Group Ltd.6�   �� 5   
   -   5        $��label3��5   J   $��25��6   $
��Left=�6   $��	12,12,0,0q�67   $	��Top=�6X   $��50��6p   .��6{   Map:6�   �� 5   
   -   5        $��comboBoxMaps��5   M   $��23��6   $
��Left=�6   $��	66,13,0,0q�6:   $	��Top=�6a   $	��247��6y   �� 5   
   -   5        $��buttonLoadMap��5   1   $��23��6   $��
319,13,0,0q�6   $	��Top=�6F   $
��Left=�6z   $��75��6�   .��6�   Load6�   �� 5   
   -	   5        $��buttonDestroy��5   L   $��23��6   $
��Left=�6   $��
464,13,0,0q�68   $	��Top=�6a   $��75��6y   .��6�   	Destroy6�   5      5      