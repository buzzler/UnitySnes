//
//  KeyInputViewController.h
//  Unity-iPhone
//
//  Created by Alexander Hodge on 2018-05-30.
//

#import <UIKit/UIKit.h>

@interface KeyInputViewController : UIViewController <UIKeyInput>

@property (nonatomic) NSArray *commands;
@property (strong, nonatomic, readwrite) UIView * inputView;

@end
