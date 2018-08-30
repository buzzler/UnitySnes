//
//  KeyInputViewController.m
//  Unity-iPhone
//
//  Created by Alexander Hodge on 2018-05-30.
//

#import "KeyInputViewController.h"
#import "ExternalInputController.h"

@interface KeyInputViewController ()

@end

@implementation KeyInputViewController

@synthesize hasText;

- (void)viewDidLoad {
    [super viewDidLoad];
    // Do any additional setup after loading the view from its nib.
    
    self.inputView = [[UIView alloc] initWithFrame: CGRectMake(0,0,0,0)];
    
    NSMutableArray *commands = [[NSMutableArray alloc] init];
    NSString *characters = @"`1234567890-=qwertyuiop[]asdfghjkl:;'zxcvbnm,./\\?_{}|><~`+!@#$%^&*()\"";
    for (NSInteger i = 0; i < characters.length; i++) {
        NSString *input = [characters substringWithRange:NSMakeRange(i, 1)];
        [commands addObject:[UIKeyCommand keyCommandWithInput:input modifierFlags:kNilOptions action:@selector(handleKeyInput:)]];
    }
    
    /* Space */
    [commands addObject:[UIKeyCommand keyCommandWithInput:@" " modifierFlags:kNilOptions action:@selector(handleKeyInput:)]];
    /* Delete */
    [commands addObject:[UIKeyCommand keyCommandWithInput:@"\b" modifierFlags:kNilOptions action:@selector(handleKeyInput:)]];
    /* Tab */
    [commands addObject:[UIKeyCommand keyCommandWithInput:@"\t" modifierFlags:kNilOptions action:@selector(handleKeyInput:)]];
    /* Enter */
    [commands addObject:[UIKeyCommand keyCommandWithInput:@"\r" modifierFlags:kNilOptions action:@selector(handleKeyInput:)]];
    /* Up */
    [commands addObject:[UIKeyCommand keyCommandWithInput:UIKeyInputUpArrow modifierFlags:kNilOptions action:@selector(handleKeyInput:)]];
    /* Down */
    [commands addObject:[UIKeyCommand keyCommandWithInput:UIKeyInputDownArrow modifierFlags:kNilOptions action:@selector(handleKeyInput:)]];
    /* Left */
    [commands addObject:[UIKeyCommand keyCommandWithInput:UIKeyInputLeftArrow modifierFlags:kNilOptions action:@selector(handleKeyInput:)]];
    /* Right */
    [commands addObject:[UIKeyCommand keyCommandWithInput:UIKeyInputRightArrow modifierFlags:kNilOptions action:@selector(handleKeyInput:)]];
    /* Esc */
    [commands addObject:[UIKeyCommand keyCommandWithInput:UIKeyInputEscape modifierFlags:kNilOptions action:@selector(handleKeyInput:)]];
    
    self.commands = commands.copy;
}

-(void)viewDidAppear:(BOOL)animated{
    [self becomeFirstResponder];
}

- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

- (void)deleteBackward {
    
}

- (void)insertText:(nonnull NSString *)text {
    // This NSLog function will log any keystrokes/characters inputted which have not been defined in the NSString in viewDidLoad above. Potentially useful for expanding the supported list of characters with non-English characters and other symbols.
    NSLog(@"KeyInputViewController detected an undefined keystroke: %@", text);
}

- (void)encodeWithCoder:(nonnull NSCoder *)aCoder {
    
}

- (void)traitCollectionDidChange:(nullable UITraitCollection *)previousTraitCollection{
    
}

- (void)didUpdateFocusInContext:(nonnull UIFocusUpdateContext *)context withAnimationCoordinator:(nonnull UIFocusAnimationCoordinator *)coordinator {
}

- (void)setNeedsFocusUpdate {
}

- (BOOL)shouldUpdateFocusInContext:(nonnull UIFocusUpdateContext *)context {
    return  false;
}

- (void)updateFocusIfNeeded {
    
}

-(BOOL) canBecomeFirstResponder{
    return YES;
}

-(void) deleteBackward: (id)sender{
    return;
}

-(BOOL) hasText{
    return YES;
}

- (NSArray *)keyCommands{
    return self.commands;
}

- (void)handleKeyInput:(UIKeyCommand *)command {
    NSString *keyInput = command.input;
    NSMutableString *inputCharacters = [[NSMutableString alloc] init];
    
    if([keyInput isEqualToString:@" "])
    {
        [inputCharacters appendFormat:@"%@", @"SPACE"];
    }
    if([keyInput isEqualToString:@"\b"])
    {
        [inputCharacters appendFormat:@"%@", @"DEL"];
    }
    if([keyInput isEqualToString:@"\t"])
    {
        [inputCharacters appendFormat:@"%@", @"TAB"];
    }
    if([keyInput isEqualToString:@"\r"])
    {
        [inputCharacters appendFormat:@"%@", @"ENTER"];
    }
    if(keyInput == UIKeyInputUpArrow)
    {
        [inputCharacters appendFormat:@"%@", @"↑"];
    }
    if(keyInput == UIKeyInputDownArrow)
    {
        [inputCharacters appendFormat:@"%@", @"↓"];
    }
    if(keyInput == UIKeyInputLeftArrow)
    {
        [inputCharacters appendFormat:@"%@", @"←"];
    }
    if(keyInput == UIKeyInputRightArrow)
    {
        [inputCharacters appendFormat:@"%@", @"→"];
    }
    if(keyInput == UIKeyInputEscape)
    {
        [inputCharacters appendFormat:@"%@", @"ESC"];
    }
    if(keyInput.length > 0 && inputCharacters.length == 0)
    {
        [inputCharacters appendFormat:@"%@", keyInput.uppercaseString];
    }
    
    UnitySendMessage("Frontend", "OnInputDetected", [inputCharacters UTF8String]);
}

@end
