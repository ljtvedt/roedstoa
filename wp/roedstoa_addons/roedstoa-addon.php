<?php
/*
Plugin Name:  Roedstoa AddOn
Plugin URI:   https://github.com/ljtvedt/roedstoa
Description:  Displays information about members based on roles, and other necessary adddons for the web page
Version:      2018-09-26
Author:       Lars Jørgen Tvedt
Author URI:   https://www.akkeri.no/
License:      GPL2
License URI:  https://www.gnu.org/licenses/gpl-2.0.html
Text Domain:  wporg
Domain Path:  /languages
*/

defined( 'ABSPATH' ) or die( 'Do not execute this script outside WordPress!' );

if ( !class_exists( 'RoedstoaAddon_Redirect' ) ) {
	
	class RoedstoaAddon_Redirect
	{
		// Redirect to home on logout
		public static function auto_redirect_after_logout(){
			wp_redirect( home_url() );
			exit();
		}		
	}
}

if ( !class_exists( 'RoedstoaAddon_Role' ) ) {
	
	class RoedstoaAddon_Role
	{
		public $id;
		public $description;
		public $order;
		
		function __construct($order, $description) {
			$this->order = $order;
			$this->description = trim($description);
			$this->id = str_replace(' ', '_', strtolower($this->description));
		}

		function compareTo($other) {
			if ($this->order < $other->order) 
				return -1;
			elseif ($this->order < $this->order)
				return 1;
			else
				return 0;
		}
	}
}

if ( !class_exists( 'RoedstoaAddon_MemberList' ) ) {
	
	class RoedstoaAddon_MemberList
	{			
		const CSS_FILE = 'css/ra_style.css';
		const MEMBERS_SHORTCUT = 'ra_members';

		const ATTR_SHOW_WHEN ='showwhen';
		const ATTR_LOGIN = 'login';
		const ATTR_EMAIL = 'email';
		const ATTR_ROLES = 'roles';
		const ATTR_FIELDS = 'fields';
		const ATTR_FIELDS_AVATAR= 'avatar';
		
		const AVATAR_SIZE = 192;
		
		// Activate Plugin
		public static function activate()
		{
			return '';
		}
		
		// Deactivate Plugin
		public static function deactivate() 
		{
			return '';
		}
		
		// Initialize Shortcodes
		public static function shortcodes_init() 
		{
			add_shortcode( self::MEMBERS_SHORTCUT, array('RoedstoaAddon_MemberList', 'members_func') );
			return '';
		}
		
		public static function load_plugin_css() 
		{
			$plugin_url = plugin_dir_url( __FILE__ );
			wp_enqueue_style( 'amb_style', $plugin_url . self::CSS_FILE );
		}
		
		// Returns an array of a rich role representation, ordered as in the string
		public static function ordered_roles ($roles) {
			$order = 0;
			$roleArray = [];
			foreach (explode(",", $roles) as $role) {
				$roleArray[] = new RoedstoaAddon_Role($order++, trim($role));
			}
			return $roleArray;
		}
							
		public static function get_users_with_roles ($roles) 
		{
			$roleArray = self::ordered_roles($roles);
			$user_query = new WP_User_Query( 
								array( 'role__in' => 
										array_map(	
											function($role) {
												return $role->id;
											},
											$roleArray) ) );
			
			return  array_map(
							function ($user) use($roleArray) { 
								$usersRoles = 	
									array_filter($roleArray, 
										function ($role) use ($user) {
											$user_meta=get_userdata($user->ID);
											$user_roles=$user_meta->roles;
											return in_array($role->id, $user_roles);
										});
								uasort($usersRoles, 
									function($a, $b) {
										return ($a.compareTo($b));
									});
								reset($usersRoles);
								$user->richRole	= current($usersRoles);
								return $user; 
							}, 
							$user_query->get_results());							
		}		
		
		public static function get_user($login, $email) 
		{
			if (isset($login) AND !is_null($login)) 
			{
				$user = get_user_by('login', $login);
				if (! is_null($user)) 
				{
					return $user;
				} 
			}
			if (isset($email) AND !is_null($email)) 
			{
				$user = get_user_by('email', $email);
				if (!is_null($user)) 
				{
					return $user;
				} 
			}
			return NULL;
		}		
				
		public static function get_requested_member_data( $user, $fields ) {
			$member_data = [];
			$field_array = explode(",", strtolower($fields));
			$meta_data = get_user_meta($user->ID);
			
			if (in_array(self::ATTR_FIELDS_AVATAR, $field_array)) {
				$member_data[] = [self::ATTR_FIELDS_AVATAR => get_avatar($user->ID, self::AVATAR_SIZE )];
			}
			
			foreach ($field_array as $field_raw) 
			{
				$field = trim($field_raw);
				if ( ! ($field === self::ATTR_FIELDS_AVATAR) ) {
					$meta_data_entry = array_key_exists($field, $meta_data) ? $meta_data[$field][0] : NULL;
					
					if (!is_null($meta_data_entry)) 
						$member_data[] = [$field => $meta_data_entry];
					elseif ($field === 'user_email') 
						$member_data[] = [$field => sprintf("<a href=mailto:%s>%s</a>", $user->user_email, $user->user_email)];
					elseif ($field === 'user_url' && strlen($user->user_url)>0) 
						$member_data[] = [$field => sprintf("<a href=%s>%s</a>",$user->user_url, $user->user_url)];
					elseif ($field === 'display_name') 
						$member_data[] = [$field => $user->display_name];
					elseif ($field === 'user_nicename') 
						$member_data[] = [$field => $user->user_nicename];					
					elseif ($field === 'role') 
						$member_data[] = [$field => $user->richRole->description];					
				}
			}
			unset($field);
			return $member_data;
		}			


		public static function build_output($atts = [], $content = null, $shortcode_tag)
		{
			$email = array_key_exists(self::ATTR_EMAIL, $atts) ? $atts[self::ATTR_EMAIL] : NULL;
			$login = array_key_exists(self::ATTR_LOGIN, $atts) ? $atts[self::ATTR_LOGIN] : NULL;
			$roles = array_key_exists(self::ATTR_ROLES, $atts) ? $atts[self::ATTR_ROLES] : NULL;
			$fields = array_key_exists(self::ATTR_FIELDS, $atts) ? $atts[self::ATTR_FIELDS] : NULL;
			
			if (isset($roles)) {
				$users = self::get_users_with_roles($roles);
				usort($users, 
					function($u1, $u2) { 
						$order = $u1->richRole->order - $u2->richRole->order;
						if ($order == 0) 
							$order = strcasecmp($u1->last_name(), $u2->last_name());
						if ($order == 0) 
							$order = strcasecmp($u1->first_name(), $u2->first_name());
						return $order;
					});
			}
			else {
				$user = self::get_user($login, $email);
				if (! is_null($user)) {
					$users = [$user];
				}
			}	
			
			$result = apply_filters('the_content', do_shortcode($content));
			$result .= "<div>";
			$showavatar = false;
			foreach ($users as $user) 
			{			
				$data = self::get_requested_member_data($user, $fields);
				$result .= "<p><div class=\"container\"><div class=\"row\">";
				foreach ( $data as $field ) 
				{
					$key = array_keys($field)[0];
					if ($key === self::ATTR_FIELDS_AVATAR)
					{
						$showavatar = true;
						$result .= sprintf("<div class=\"col-12 col-sm-4 text-center text-sm-left\">%s</div><div class=\"col col-sm-8 text-center text-sm-left\">", $field[$key]);
					}
					elseif (isset($key) AND isset($field[$key])) 
					{
						$result .= sprintf("<p class=\"ra-%s\" style=\"margin-bottom:0em; margin-top:0em\">%s</p>", $key, $field[$key]);
					}
				}
				$result .= "</div></div></p>";
				if ($showavatar) 
				{
					$result .= "</div>";
				}
				$result .= "</p>";
			}
			$result .= "</div>";
			return($result);
		}		

		public static function members_func($atts = [], $content = null, $shortcode_tag)
		{
			$showwhen = array_key_exists(self::ATTR_SHOW_WHEN, $atts) ? $atts[self::ATTR_SHOW_WHEN] : NULL;
			if (is_null($showwhen) || 
			   ($showwhen == 'notloggedin' && !is_user_logged_in()) ||
			   ($showwhen == 'loggedin' && is_user_logged_in()))
			{				
				return (self::build_output ($atts, $content, $shortcode_tag));
			}
		}
	}
}

register_activation_hook( __FILE__, 'RoedstoaAddon_MemberList::activate' );
register_deactivation_hook( __FILE__, 'RoedstoaAddon_MemberList::deactivate' );

add_action('init', 'RoedstoaAddon_MemberList::shortcodes_init');
add_action( 'wp_enqueue_scripts', 'RoedstoaAddon_MemberList::load_plugin_css', 10000 );

add_action('wp_logout','RoedstoaAddon_Redirect::auto_redirect_after_logout');

?>