//
//  ExternalInputController.h
//
//
//  Created by Alexander Hodge on 2018-06-03.
//
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "KeyInputViewController.h"

@interface ExternalInputController : NSObject {
}

@property(nonatomic, retain)KeyInputViewController *keyInput;

+(ExternalInputController*)GameController;

-(void)setupExternalInput;

@end

