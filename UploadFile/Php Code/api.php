<?php

require_once("Rest.inc.php");

class API extends REST {
	
	public $data = "";
  private $uploading_path = "";

	public function __construct(){
			parent::__construct();				// Init parent contructor
			
      $this->uploading_path = getcwd()."/upload";
		}
		/*
		 * Public method for access api.
		 * This method dynmically call the method based on the query string
		 *
		 */
		public function processApi(){
			$func = strtolower(trim(str_replace("/","",$_REQUEST['rquest'])));
			if((int)method_exists($this,$func) > 0)
				$this->$func();
			else
				$this->response('',404);				// If the method not exist with in this class, response would be "Page not found".
		}
		 
		private function test()
		{
			if($this->get_request_method() != "POST"){
				$this->response('',406);
			}
			try{

				$pid = $this->_request['param1'];
				$did = $this->_request['param2'];
				$filename = round(microtime(true));
				$pdfFilename = $filename . ".pdf";
        move_uploaded_file($_FILES["file"]["tmp_name"], $this->uploading_path . $pdfFilename );
				$success = array('status' => "success", "msg" => "File uploaded.");
				$this->response($this->json($success),200);	
				
			}
			catch(Exception $e)
			{
				$error = array('status' => false, "msg" => 'Caught exception: ' .  $e->getMessage());
				$this->response($this->json($error),200);
			}
		}
		/*
		 *	Encode array into JSON
		*/
		private function json($data){
			if(is_array($data)){
				return json_encode($data);
			}
		}
	}
	
	// Initiiate Library
	
	$api = new API;
	$api->processApi();
	?>